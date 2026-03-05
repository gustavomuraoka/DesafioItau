using CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class RebalanceamentoRepository : IRebalanceamentoRepository
{
    private readonly AppDbContext _context;

    public RebalanceamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Rebalanceamento>> GetByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default)
        => await _context.Rebalanceamentos
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.DataRebalanceamento)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Rebalanceamento rebalanceamento, CancellationToken cancellationToken = default)
        => await _context.Rebalanceamentos.AddAsync(rebalanceamento, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Rebalanceamento> rebalanceamentos, CancellationToken cancellationToken = default)
        => await _context.Rebalanceamentos.AddRangeAsync(rebalanceamentos, cancellationToken);
}
