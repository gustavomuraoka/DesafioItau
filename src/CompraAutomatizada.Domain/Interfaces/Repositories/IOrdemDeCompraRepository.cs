using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface IOrdemDeCompraRepository
{
    Task<OrdemDeCompra?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(OrdemDeCompra ordem, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<OrdemDeCompra> ordens, CancellationToken cancellationToken = default);
    Task<IEnumerable<Distribuicao>> GetDistribuicoesByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default);
}
