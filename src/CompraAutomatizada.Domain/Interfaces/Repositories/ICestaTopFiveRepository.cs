using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface ICestaTopFiveRepository
{
    Task<CestaTopFive?> GetAtivaAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CestaTopFive>> GetHistoricoAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CestaTopFive cesta, CancellationToken cancellationToken = default);
    Task UpdateAsync(CestaTopFive cesta, CancellationToken cancellationToken = default);
}
