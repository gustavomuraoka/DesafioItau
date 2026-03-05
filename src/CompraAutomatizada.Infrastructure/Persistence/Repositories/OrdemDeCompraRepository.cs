using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class OrdemDeCompraRepository : IOrdemDeCompraRepository
{
    private readonly AppDbContext _context;

    public OrdemDeCompraRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrdemDeCompra?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _context.OrdensCompra
            .Include(o => o.Distribuicoes)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IEnumerable<Distribuicao>> GetDistribuicoesByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default)
        => await _context.Distribuicoes
            .Where(d => d.ClienteId == clienteId)
            .OrderBy(d => d.DataDistribuicao)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(OrdemDeCompra ordem, CancellationToken cancellationToken = default)
        => await _context.OrdensCompra.AddAsync(ordem, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<OrdemDeCompra> ordens, CancellationToken cancellationToken = default)
        => await _context.OrdensCompra.AddRangeAsync(ordens, cancellationToken);
}
