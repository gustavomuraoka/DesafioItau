using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface IContaGraficaRepository
{
    Task<ContaGrafica?> GetMasterAsync(CancellationToken cancellationToken = default);
    Task<ContaGrafica?> GetByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default);
    Task<ContaGrafica?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(ContaGrafica conta, CancellationToken cancellationToken = default);
    Task UpdateAsync(ContaGrafica conta, CancellationToken cancellationToken = default);
}
