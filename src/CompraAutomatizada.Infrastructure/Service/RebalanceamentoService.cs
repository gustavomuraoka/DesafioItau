using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CompraAutomatizada.Infrastructure.Services;

public class RebalanceamentoService : IRebalanceamentoService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ICotacaoService _cotacaoService;
    private readonly ICustodiaService _custodiaService;
    private readonly IIRService _irService;
    private readonly IRebalanceamentoRepository _rebalanceamentoRepository;
    private readonly ILogger<RebalanceamentoService> _logger;

    public RebalanceamentoService(
        IClienteRepository clienteRepository,
        ICotacaoService cotacaoService,
        ICustodiaService custodiaService,
        IIRService irService,
        IRebalanceamentoRepository rebalanceamentoRepository,
        ILogger<RebalanceamentoService> logger)
    {
        _clienteRepository = clienteRepository;
        _cotacaoService = cotacaoService;
        _custodiaService = custodiaService;
        _irService = irService;
        _rebalanceamentoRepository = rebalanceamentoRepository;
        _logger = logger;
    }

    public async Task RebalancearPorMudancaDeCestaAsync(CestaTopFive cestaAntiga, CestaTopFive novaCesta, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rebalanceamento por mudança de cesta iniciado.");

        var clientes = (await _clienteRepository.GetAtivosAsync(cancellationToken)).ToList();
        if (!clientes.Any()) return;

        var tickersAntigos = cestaAntiga.Itens.Select(i => i.Ticker).ToHashSet();
        var tickersNovos = novaCesta.Itens.Select(i => i.Ticker).ToHashSet();
        var tickersRemovidos = tickersAntigos.Except(tickersNovos).ToList();
        var tickersAdicionados = tickersNovos.Except(tickersAntigos).ToList();

        var todosTickers = tickersAntigos.Union(tickersNovos).ToList();
        var cotacoes = await _cotacaoService.ObterCotacoesFechamentoAsync(todosTickers, cancellationToken);

        foreach (var cliente in clientes)
        {
            try
            {
                // 1. Vender ativos removidos da cesta
                foreach (var ticker in tickersRemovidos)
                {
                    var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);
                    if (cotacao <= 0) continue;

                    var conta = await _clienteRepository.GetByIdAsync(cliente.Id, cancellationToken);
                    var posicao = conta?.Conta?.Posicoes.FirstOrDefault(p => p.Ticker == ticker);
                    if (posicao is null || posicao.Quantidade <= 0) continue;

                    var precoMedio = await _custodiaService.VenderDaFilhoteAsync(
                        cliente.Id, ticker, posicao.Quantidade, cotacao, cancellationToken);

                    await _irService.ProcessarIRVendaAsync(
                        cliente.Id, ticker, posicao.Quantidade, cotacao, precoMedio, cancellationToken);
                }

                // 2. Comprar ativos adicionados à cesta
                foreach (var ticker in tickersAdicionados)
                {
                    var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);
                    if (cotacao <= 0) continue;

                    var itemNovoCesta = novaCesta.Itens.First(i => i.Ticker == ticker);
                    var valorMensalCliente = cliente.ValorMensal;
                    var valorAlocado = Math.Round(valorMensalCliente * (itemNovoCesta.Percentual.Valor / 100m), 2);
                    var qtd = (int)Math.Floor(valorAlocado / cotacao);

                    if (qtd <= 0) continue;

                    await _custodiaService.DistribuirParaFilhoteAsync(cliente.Id, ticker, qtd, cotacao, cancellationToken);
                    await _irService.ProcessarDedoDuroAsync(cliente.Id, ticker, qtd, cotacao, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao rebalancear cliente {ClienteId}. Continuando.", cliente.Id);
            }
        }

        _logger.LogInformation("Rebalanceamento concluído para {N} clientes.", clientes.Count);
    }

    public async Task RebalancearPorDesvioAsync(CancellationToken cancellationToken = default)
    {
        // Implementação futura — verifica desvio de proporção e reequilibra
        _logger.LogInformation("Rebalanceamento por desvio ainda não implementado.");
        await Task.CompletedTask;
    }
}
