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

        var todosTickers = tickersAntigos.Union(tickersNovos).ToList();
        var cotacoes = await _cotacaoService.ObterCotacoesFechamentoAsync(todosTickers, null, cancellationToken);

        foreach (var cliente in clientes)
        {
            try
            {
                var clienteComConta = await _clienteRepository.GetByIdAsync(cliente.Id, cancellationToken);
                var posicoes = clienteComConta?.Conta?.Posicoes.ToList() ?? new();

                decimal valorLiberado = 0m;

                foreach (var ticker in tickersRemovidos)
                {
                    var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);
                    if (cotacao <= 0) continue;

                    var posicao = posicoes.FirstOrDefault(p => p.Ticker == ticker);
                    if (posicao is null || posicao.Quantidade <= 0) continue;

                    var precoMedio = await _custodiaService.VenderDaFilhoteAsync(
                        cliente.Id, ticker, posicao.Quantidade, cotacao, cancellationToken);

                    valorLiberado += Math.Round(posicao.Quantidade * cotacao, 2);

                    await _irService.ProcessarIRVendaAsync(
                        cliente.Id, ticker, posicao.Quantidade, cotacao, precoMedio, cancellationToken);

                    _logger.LogInformation("Cliente {ClienteId}: vendeu {Qtd} {Ticker} @ {Cotacao}. Liberado: {Valor}",
                        cliente.Id, posicao.Quantidade, ticker, cotacao, valorLiberado);
                }

                decimal valorMantido = 0m;
                foreach (var ticker in tickersNovos.Intersect(tickersAntigos))
                {
                    var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);
                    if (cotacao <= 0) continue;

                    var posicao = posicoes.FirstOrDefault(p => p.Ticker == ticker);
                    if (posicao is null) continue;

                    valorMantido += Math.Round(posicao.Quantidade * cotacao, 2);
                }

                var valorTotal = valorLiberado + valorMantido;

                if (valorTotal <= 0)
                {
                    _logger.LogWarning("Cliente {ClienteId}: valor total zerado, pulando.", cliente.Id);
                    continue;
                }

                foreach (var item in novaCesta.Itens)
                {
                    var ticker = item.Ticker;
                    var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);
                    if (cotacao <= 0) continue;

                    var percentual = item.Percentual.Valor / 100m;
                    var valorAlvo = Math.Round(valorTotal * percentual, 2);

                    var posicaoAtual = posicoes.FirstOrDefault(p => p.Ticker == ticker);
                    var qtdAtual = posicaoAtual?.Quantidade ?? 0;
                    var valorAtual = Math.Round(qtdAtual * cotacao, 2);
                    var valorDelta = valorAlvo - valorAtual;

                    if (valorDelta >= cotacao)
                    {
                        var qtdComprar = (int)Math.Floor(valorDelta / cotacao);
                        if (qtdComprar <= 0) continue;

                        await _custodiaService.DistribuirParaFilhoteAsync(
                            cliente.Id, ticker, qtdComprar, cotacao, cancellationToken);
                        await _irService.ProcessarDedoDuroAsync(
                            cliente.Id, ticker, qtdComprar, cotacao, cancellationToken);

                        _logger.LogInformation("Cliente {ClienteId}: comprou {Qtd} {Ticker} @ {Cotacao}.",
                            cliente.Id, qtdComprar, ticker, cotacao);
                    }
                    else if (valorDelta <= -cotacao)
                    {
                        var qtdVender = (int)Math.Floor(Math.Abs(valorDelta) / cotacao);
                        if (qtdVender <= 0 || qtdVender > qtdAtual) continue;

                        var precoMedio = await _custodiaService.VenderDaFilhoteAsync(
                            cliente.Id, ticker, qtdVender, cotacao, cancellationToken);
                        await _irService.ProcessarIRVendaAsync(
                            cliente.Id, ticker, qtdVender, cotacao, precoMedio, cancellationToken);

                        _logger.LogInformation("Cliente {ClienteId}: vendeu excesso {Qtd} {Ticker} @ {Cotacao}.",
                            cliente.Id, qtdVender, ticker, cotacao);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao rebalancear cliente {ClienteId}. Continuando.", cliente.Id);
            }
        }

        _logger.LogInformation("Rebalanceamento concluído para {N} clientes.", clientes.Count);
    }

    public async Task RebalancearPorDesvioAsync(
        IEnumerable<long> clienteIds,
        CestaTopFive cesta,
        IReadOnlyDictionary<string, decimal> cotacoes,
        CancellationToken cancellationToken = default)
    {
        var tickers = cesta.Itens.Select(i => i.Ticker).ToList();

        foreach (var clienteId in clienteIds)
        {
            try
            {
                var clienteComConta = await _clienteRepository.GetByIdAsync(clienteId, cancellationToken);
                var posicoes = clienteComConta?.Conta?.Posicoes.ToList() ?? new();
                if (!posicoes.Any()) continue;

                var valorTotalCarteira = posicoes
                    .Where(p => tickers.Contains(p.Ticker))
                    .Sum(p => p.Quantidade * cotacoes.GetValueOrDefault(p.Ticker, 0m));

                if (valorTotalCarteira <= 0) continue;

                // Primeiro passa: identifica vendas (ativos acima do alvo)
                decimal caixaGerado = 0m;
                var vendas = new List<(string Ticker, int Qtd, decimal Cotacao, decimal PrecoMedio)>();
                var compras = new List<(string Ticker, int Qtd, decimal Cotacao)>();

                foreach (var item in cesta.Itens)
                {
                    var ticker = item.Ticker;
                    var cotacao = cotacoes.GetValueOrDefault(ticker, 0m);
                    if (cotacao <= 0) continue;

                    var percentualAlvo = item.Percentual.Valor / 100m;
                    var valorAlvo = Math.Round(valorTotalCarteira * percentualAlvo, 2);

                    var posicao = posicoes.FirstOrDefault(p => p.Ticker == ticker);
                    var qtdAtual = posicao?.Quantidade ?? 0;
                    var valorAtual = Math.Round(qtdAtual * cotacao, 2);
                    var valorDelta = valorAlvo - valorAtual;

                    if (valorDelta <= -cotacao)
                    {
                        var qtdVender = (int)Math.Floor(Math.Abs(valorDelta) / cotacao);
                        if (qtdVender <= 0 || qtdVender > qtdAtual) continue;

                        vendas.Add((ticker, qtdVender, cotacao, posicao!.PrecoMedio));
                        caixaGerado += Math.Round(qtdVender * cotacao, 2);
                    }
                    else if (valorDelta >= cotacao)
                    {
                        var qtdComprar = (int)Math.Floor(valorDelta / cotacao);
                        if (qtdComprar > 0)
                            compras.Add((ticker, qtdComprar, cotacao));
                    }
                }

                // Segundo passo: executa vendas
                foreach (var (ticker, qtd, cotacao, precoMedio) in vendas)
                {
                    await _custodiaService.VenderDaFilhoteAsync(clienteId, ticker, qtd, cotacao, cancellationToken);
                    await _irService.ProcessarIRVendaAsync(clienteId, ticker, qtd, cotacao, precoMedio, cancellationToken);
                    _logger.LogInformation("Rebalanceamento desvio — Cliente {Id}: vendeu {Qtd} {Ticker} @ {Cotacao}.",
                        clienteId, qtd, ticker, cotacao);
                }

                // Terceiro passo: executa compras limitadas ao caixa gerado pelas vendas
                decimal caixoRestante = caixaGerado;
                foreach (var (ticker, qtdDesejado, cotacao) in compras)
                {
                    if (caixoRestante < cotacao) break;

                    var qtdPossivel = (int)Math.Floor(caixoRestante / cotacao);
                    var qtdComprar = Math.Min(qtdDesejado, qtdPossivel);
                    if (qtdComprar <= 0) continue;

                    await _custodiaService.DistribuirParaFilhoteAsync(clienteId, ticker, qtdComprar, cotacao, cancellationToken);
                    await _irService.ProcessarDedoDuroAsync(clienteId, ticker, qtdComprar, cotacao, cancellationToken);

                    caixoRestante -= Math.Round(qtdComprar * cotacao, 2);

                    _logger.LogInformation("Rebalanceamento desvio — Cliente {Id}: comprou {Qtd} {Ticker} @ {Cotacao}. Caixa restante: {Caixa}.",
                        clienteId, qtdComprar, ticker, cotacao, caixoRestante);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no rebalanceamento por desvio do cliente {ClienteId}.", clienteId);
            }
        }
    }
}
