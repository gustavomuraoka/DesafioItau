using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _context.Clientes
            .Include(c => c.Conta)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await _context.Clientes
            .Include(c => c.Conta)
            .FirstOrDefaultAsync(c => c.Cpf.Valor == cpf, cancellationToken);

    public async Task<IEnumerable<Cliente>> GetAtivosAsync(CancellationToken cancellationToken = default)
        => await _context.Clientes
            .Include(c => c.Conta)
            .Where(c => c.Ativo)
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }


    public async Task<int> CountAtivosAsync(CancellationToken cancellationToken = default)
        => await _context.Clientes.CountAsync(c => c.Ativo, cancellationToken);

    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
        => await _context.Clientes.AddAsync(cliente, cancellationToken);

    public async Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _context.Clientes.Update(cliente);
        await Task.CompletedTask;
    }
}
