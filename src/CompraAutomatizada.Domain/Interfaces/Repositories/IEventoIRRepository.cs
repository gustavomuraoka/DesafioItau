using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;

namespace CompraAutomatizada.Domain.Interfaces.Repositories;

public interface IEventoIRRepository
{
	Task<IEnumerable<EventoIR>> GetNaoPublicadosAsync(CancellationToken cancellationToken = default);
	Task<decimal> GetTotalVendasNoMesAsync(long clienteId, int ano, int mes, CancellationToken cancellationToken = default);
	Task AddAsync(EventoIR evento, CancellationToken cancellationToken = default);
	Task UpdateAsync(EventoIR evento, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
