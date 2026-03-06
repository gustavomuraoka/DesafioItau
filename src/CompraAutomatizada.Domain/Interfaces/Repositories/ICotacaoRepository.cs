using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface ICotacaoRepository
{
    Task<Cotacao?> GetUltimaByTickerAsync(string ticker, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cotacao>> GetUltimasByTickersAsync(IEnumerable<string> tickers, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cotacao>> GetUltimasByTickersAteDataAsync(IEnumerable<string> tickers, DateOnly dataReferencia, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Cotacao> cotacoes, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteCotacaoParaDataAsync(string ticker, DateOnly data, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetTickersParaDataAsync(DateOnly data, CancellationToken cancellationToken = default);
}
