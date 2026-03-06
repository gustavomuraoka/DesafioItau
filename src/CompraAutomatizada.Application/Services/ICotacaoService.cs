using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;

namespace CompraAutomatizada.Application.Services;

public interface ICotacaoService
{
    Task<Cotacao?> ObterUltimaByTickerAsync(string ticker, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, decimal>> ObterCotacoesFechamentoAsync(IEnumerable<string> tickers, DateOnly? dataReferencia = null, CancellationToken cancellationToken = default);
    Task ImportarCotahistAsync(string caminhoArquivo, CancellationToken cancellationToken = default);
    Task ImportarPastaCotacoesAsync(CancellationToken cancellationToken = default);

}
