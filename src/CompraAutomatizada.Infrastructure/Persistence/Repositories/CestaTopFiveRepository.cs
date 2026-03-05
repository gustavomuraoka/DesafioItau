using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class CestaTopFiveRepository : ICestaTopFiveRepository
{
    private readonly AppDbContext _context;

    public CestaTopFiveRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CestaTopFive?> GetAtivaAsync(CancellationToken cancellationToken = default)
        => await _context.CestasRecomendacao
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Ativa, cancellationToken);

    public async Task<IEnumerable<CestaTopFive>> GetHistoricoAsync(CancellationToken cancellationToken = default)
        => await _context.CestasRecomendacao
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CestaTopFive cesta, CancellationToken cancellationToken = default)
        => await _context.CestasRecomendacao.AddAsync(cesta, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task UpdateAsync(CestaTopFive cesta, CancellationToken cancellationToken = default)
    {
        _context.CestasRecomendacao.Update(cesta);
        await Task.CompletedTask;
    }
}
