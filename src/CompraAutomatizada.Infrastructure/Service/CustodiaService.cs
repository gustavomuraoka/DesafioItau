using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CompraAutomatizada.Infrastructure.Services;

public class CustodiaService : ICustodiaService
{
    private readonly IContaGraficaRepository _contaRepository;
    private readonly ILogger<CustodiaService> _logger;

    public CustodiaService(
        IContaGraficaRepository contaRepository,
        ILogger<CustodiaService> logger)
    {
        _contaRepository = contaRepository;
        _logger = logger;
    }

    public async Task AdicionarNaMasterAsync(string ticker, int quantidade, decimal precoUnitario, CancellationToken cancellationToken = default)
    {
        var master = await _contaRepository.GetMasterAsync(cancellationToken)
            ?? throw new DomainException("Conta master não encontrada.");

        master.AdicionarOuAtualizarPosicao(ticker, quantidade, precoUnitario);
        await _contaRepository.UpdateAsync(master, cancellationToken);

        _logger.LogInformation("Master +{Qtd} {Ticker} @ {Preco}", quantidade, ticker, precoUnitario);
    }

    public async Task DistribuirParaFilhoteAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken cancellationToken = default)
    {
        var filhote = await _contaRepository.GetByClienteIdAsync(clienteId, cancellationToken)
            ?? throw new DomainException($"Conta filhote não encontrada para cliente {clienteId}.");

        filhote.AdicionarOuAtualizarPosicao(ticker, quantidade, precoUnitario);
        await _contaRepository.UpdateAsync(filhote, cancellationToken);
    }

    public async Task DescontarDaMasterAsync(string ticker, int quantidade, CancellationToken cancellationToken = default)
    {
        var master = await _contaRepository.GetMasterAsync(cancellationToken)
            ?? throw new DomainException("Conta master não encontrada.");

        master.RemoverPosicao(ticker, quantidade);
        await _contaRepository.UpdateAsync(master, cancellationToken);
    }

    public async Task<int> ObterSaldoMasterAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var master = await _contaRepository.GetMasterAsync(cancellationToken);
        if (master is null) return 0;

        var posicao = master.Posicoes.FirstOrDefault(p => p.Ticker == ticker.ToUpper());
        return posicao?.Quantidade ?? 0;
    }

    public async Task<decimal> VenderDaFilhoteAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, CancellationToken cancellationToken = default)
    {
        var filhote = await _contaRepository.GetByClienteIdAsync(clienteId, cancellationToken)
            ?? throw new DomainException($"Conta filhote não encontrada para cliente {clienteId}.");

        var posicao = filhote.Posicoes.FirstOrDefault(p => p.Ticker == ticker.ToUpper())
            ?? throw new DomainException($"Cliente {clienteId} não possui posição em {ticker}.");

        var precoMedioAquisicao = posicao.PrecoMedio;

        filhote.RemoverPosicao(ticker, quantidade);
        await _contaRepository.UpdateAsync(filhote, cancellationToken);

        return precoMedioAquisicao;
    }
}
