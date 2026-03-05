using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cliente>> GetAtivosAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<int> CountAtivosAsync(CancellationToken cancellationToken = default);

}
