using CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface IRebalanceamentoRepository
{
    Task<IEnumerable<Rebalanceamento>> GetByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default);
    Task AddAsync(Rebalanceamento rebalanceamento, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Rebalanceamento> rebalanceamentos, CancellationToken cancellationToken = default);
}
