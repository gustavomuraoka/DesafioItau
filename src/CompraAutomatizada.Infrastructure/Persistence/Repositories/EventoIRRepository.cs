using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class EventoIRRepository : IEventoIRRepository
{
    private readonly AppDbContext _context;

    public EventoIRRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventoIR>> GetNaoPublicadosAsync(CancellationToken cancellationToken = default)
        => await _context.EventosIR
            .Where(e => !e.PublicadoKafka)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalVendasNoMesAsync(long clienteId, int ano, int mes, CancellationToken cancellationToken = default)
        => await _context.EventosIR
            .Where(e => e.ClienteId == clienteId
                && e.DataEvento.Year == ano
                && e.DataEvento.Month == mes
                && e.Tipo == Domain.Enums.TipoEventoIR.IrVenda)
            .SumAsync(e => e.ValorOperacao, cancellationToken);

    public async Task AddAsync(EventoIR evento, CancellationToken cancellationToken = default)
        => await _context.EventosIR.AddAsync(evento, cancellationToken);

    public Task UpdateAsync(EventoIR evento, CancellationToken cancellationToken = default)
    {
        _context.EventosIR.Update(evento);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
