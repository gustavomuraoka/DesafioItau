using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class CotacaoRepository : ICotacaoRepository
{
    private readonly AppDbContext _context;

    public CotacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cotacao?> GetUltimaByTickerAsync(string ticker, CancellationToken cancellationToken = default)
        => await _context.Cotacoes
            .Where(c => c.Ticker == ticker.ToUpper())
            .OrderByDescending(c => c.DataPregao)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<Cotacao>> GetUltimasByTickersAsync(IEnumerable<string> tickers, CancellationToken cancellationToken = default)
    {
        var tickersUpper = tickers.Select(t => t.ToUpper()).ToList();

        return await _context.Cotacoes
            .Where(c => tickersUpper.Contains(c.Ticker))
            .GroupBy(c => c.Ticker)
            .Select(g => g.OrderByDescending(c => c.DataPregao).First())
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<string>> GetTickersParaDataAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var tickers = await _context.Cotacoes
            .Where(c => c.DataPregao == data)
            .Select(c => c.Ticker)
            .ToListAsync(cancellationToken);
        return tickers.ToHashSet();
    }

    public async Task<IEnumerable<Cotacao>> GetUltimasByTickersAteDataAsync(
        IEnumerable<string> tickers,
        DateOnly dataReferencia,
        CancellationToken cancellationToken = default)
    {
        var tickersList = tickers.Select(t => t.ToUpper()).ToList();

        return await _context.Cotacoes
            .Where(c => tickersList.Contains(c.Ticker) && c.DataPregao <= dataReferencia)
            .GroupBy(c => c.Ticker)
            .Select(g => g.OrderByDescending(c => c.DataPregao).First())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteCotacaoParaDataAsync(string ticker, DateOnly data, CancellationToken cancellationToken = default)
        => await _context.Cotacoes
            .AnyAsync(c => c.Ticker == ticker.ToUpper() && c.DataPregao == data, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Cotacao> cotacoes, CancellationToken cancellationToken = default)
        => await _context.Cotacoes.AddRangeAsync(cotacoes, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
