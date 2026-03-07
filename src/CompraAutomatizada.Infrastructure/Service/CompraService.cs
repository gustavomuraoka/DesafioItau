using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CompraAutomatizada.Infrastructure.Services;

public class CompraService : ICompraService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ICestaTopFiveRepository _cestaRepository;
    private readonly IOrdemDeCompraRepository _ordemRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICotacaoService _cotacaoService;
    private readonly ICustodiaService _custodiaService;
    private readonly IIRService _irService;
    private readonly IRebalanceamentoService _rebalanceamentoService;
    private readonly ILogger<CompraService> _logger;

    public CompraService(
        IClienteRepository clienteRepository,
        ICestaTopFiveRepository cestaRepository,
        IOrdemDeCompraRepository ordemRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICotacaoService cotacaoService,
        ICustodiaService custodiaService,
        IIRService irService,
        IRebalanceamentoService rebalanceamentoService,
        ILogger<CompraService> logger)
    {
        _clienteRepository = clienteRepository;
        _cestaRepository = cestaRepository;
        _ordemRepository = ordemRepository;
        _contaGraficaRepository = contaGraficaRepository;
        _cotacaoService = cotacaoService;
        _custodiaService = custodiaService;
        _irService = irService;
        _rebalanceamentoService = rebalanceamentoService;
        _logger = logger;
    }

    public async Task<ExecucaoCompraDto> ExecutarAsync(DateOnly dataReferencia, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando compra programada para {Data}", dataReferencia);

        var cesta = await _cestaRepository.GetAtivaAsync(cancellationToken)
            ?? throw new DomainException("Nenhuma cesta ativa encontrada.");

        var clientes = (await _clienteRepository.GetAtivosAsync(cancellationToken)).ToList();
        if (!clientes.Any())
            throw new DomainException("Nenhum cliente ativo encontrado.");

        var valorPorCliente = clientes.ToDictionary(c => c.Id, c => Math.Round(c.ValorMensal / 3, 2));
        var totalConsolidado = valorPorCliente.Values.Sum();

        var contasFilhotes = new Dictionary<long, long>();
        foreach (var cliente in clientes)
        {
            var conta = await _contaGraficaRepository.GetByClienteIdAsync(cliente.Id, cancellationToken);
            if (conta is not null)
                contasFilhotes[cliente.Id] = conta.Id;
        }

        var tickers = cesta.Itens.Select(i => i.Ticker).ToList();
        var cotacoes = await _cotacaoService.ObterCotacoesFechamentoAsync(tickers, dataReferencia, cancellationToken);

        var ordensCompraDto = new List<OrdemCompraDto>();
        var distribuicoesDto = new List<DistribuicaoClienteDto>();
        var residuos = new List<ResiduoDto>();
        int totalEventosIR = 0;

        foreach (var item in cesta.Itens)
        {
            var ticker = item.Ticker;
            var percentual = item.Percentual.Valor / 100m;
            var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);

            if (cotacao <= 0)
            {
                _logger.LogWarning("Cotação não encontrada para {Ticker}. Pulando.", ticker);
                continue;
            }

            var valorTotalAtivo = Math.Round(totalConsolidado * percentual, 2);
            var saldoMaster = await _custodiaService.ObterSaldoMasterAsync(ticker, cancellationToken);
            var qtdBruta = (int)Math.Floor(valorTotalAtivo / cotacao);
            var qtdComprar = Math.Max(0, qtdBruta - saldoMaster);

            if (qtdComprar <= 0) continue;

            await _custodiaService.AdicionarNaMasterAsync(ticker, qtdComprar, cotacao, cancellationToken);

            var ordens = OrdemDeCompra.CriarComSplit(ContaGrafica.IdContaMaster, ticker, qtdComprar, cotacao).ToList();
            var ordemPrincipal = ordens.First();

            foreach (var ordem in ordens)
                await _ordemRepository.AddAsync(ordem, cancellationToken);
            await _ordemRepository.SaveChangesAsync(cancellationToken);

            var lotes = (qtdComprar / 100) * 100;
            var fracionario = qtdComprar % 100;

            var detalhes = new List<DetalheOrdemDto>();
            if (lotes > 0)
                detalhes.Add(new DetalheOrdemDto("LOTE", ticker, lotes));
            if (fracionario > 0)
                detalhes.Add(new DetalheOrdemDto("FRACIONARIO", $"{ticker}F", fracionario));

            ordensCompraDto.Add(new OrdemCompraDto(
                ticker,
                qtdComprar,
                detalhes,
                cotacao,
                Math.Round(qtdComprar * cotacao, 2)
            ));

            var qtdTotalDistribuida = 0;

            foreach (var cliente in clientes)
            {
                var valorCliente = valorPorCliente[cliente.Id];
                var proporcao = totalConsolidado > 0 ? valorCliente / totalConsolidado : 0m;
                var qtdCliente = (int)Math.Floor(qtdComprar * proporcao);

                if (qtdCliente <= 0) continue;
                if (!contasFilhotes.TryGetValue(cliente.Id, out var contaFilhoteId)) continue;

                await _custodiaService.DistribuirParaFilhoteAsync(cliente.Id, ticker, qtdCliente, cotacao, cancellationToken);
                await _irService.ProcessarDedoDuroAsync(cliente.Id, ticker, qtdCliente, cotacao, cancellationToken);

                ordemPrincipal.AdicionarDistribuicao(contaFilhoteId, cliente.Id, qtdCliente, cotacao, dataReferencia.ToDateTime(TimeOnly.MinValue));

                qtdTotalDistribuida += qtdCliente;
                totalEventosIR++;

                var distCliente = distribuicoesDto.FirstOrDefault(d => d.ClienteId == cliente.Id);
                if (distCliente is null)
                {
                    distCliente = new DistribuicaoClienteDto(cliente.Id, cliente.Nome, valorCliente, new List<AtivoDistribuidoDto>());
                    distribuicoesDto.Add(distCliente);
                }
                ((List<AtivoDistribuidoDto>)distCliente.Ativos).Add(new AtivoDistribuidoDto(ticker, qtdCliente));
            }

            if (qtdTotalDistribuida > 0)
                await _custodiaService.DescontarDaMasterAsync(ticker, qtdTotalDistribuida, cancellationToken);

            var residuo = qtdComprar - qtdTotalDistribuida;
            if (residuo > 0)
                residuos.Add(new ResiduoDto(ticker, residuo));

            await _ordemRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Iniciando rebalanceamento por desvio pós-compra.");
        await _rebalanceamentoService.RebalancearPorDesvioAsync(
            clientes.Select(c => c.Id),
            cesta,
            cotacoes,
            cancellationToken);

        _logger.LogInformation("Compra concluída. Clientes: {N}, Total: {Total}", clientes.Count, totalConsolidado);

        return new ExecucaoCompraDto(
            DateTime.UtcNow,
            clientes.Count,
            totalConsolidado,
            ordensCompraDto,
            distribuicoesDto,
            residuos,
            totalEventosIR,
            $"Compra programada executada com sucesso para {clientes.Count} clientes."
        );
    }
}
