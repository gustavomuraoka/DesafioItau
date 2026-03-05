using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence.Repositories;

public class ContaGraficaRepository : IContaGraficaRepository
{
    private readonly AppDbContext _context;

    public ContaGraficaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContaGrafica?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<ContaGrafica?> GetByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default)
        => await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .FirstOrDefaultAsync(c => EF.Property<long>(c, "ClienteId") == clienteId, cancellationToken);

    public async Task<ContaGrafica?> GetMasterAsync(CancellationToken cancellationToken = default)
        => await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .FirstOrDefaultAsync(c => c.Tipo == TipoConta.Master, cancellationToken);

    public async Task AddAsync(ContaGrafica conta, CancellationToken cancellationToken = default)
        => await _context.ContasGraficas.AddAsync(conta, cancellationToken);

    public async Task UpdateAsync(ContaGrafica conta, CancellationToken cancellationToken = default)
    {
        _context.ContasGraficas.Update(conta);
        await Task.CompletedTask;
    }
}
