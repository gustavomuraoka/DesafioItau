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
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<ContaGrafica?> GetByClienteIdAsync(long clienteId, CancellationToken cancellationToken = default)
        => await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Property<long>(c, "ClienteId") == clienteId, cancellationToken);

    public async Task<ContaGrafica?> GetMasterAsync(CancellationToken cancellationToken = default)
        => await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Tipo == TipoConta.Master, cancellationToken);

    public async Task AddAsync(ContaGrafica conta, CancellationToken cancellationToken = default)
        => await _context.ContasGraficas.AddAsync(conta, cancellationToken);

    public async Task UpdateAsync(ContaGrafica conta, CancellationToken cancellationToken = default)
    {
        await _context.ContasGraficas
            .Where(c => c.Id == conta.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Ativo, conta.Ativo)
                .SetProperty(c => c.NumeroConta, conta.NumeroConta),
                cancellationToken);

        var idsNoBanco = await _context.Custodias
            .Where(c => c.ContaGraficaId == conta.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var idsEmMemoria = conta.Posicoes.Select(p => p.Id).ToHashSet();

        var idsParaDeletar = idsNoBanco.Where(id => !idsEmMemoria.Contains(id)).ToList();
        if (idsParaDeletar.Any())
            await _context.Custodias
                .Where(c => idsParaDeletar.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);

        foreach (var posicao in conta.Posicoes)
        {
            if (idsNoBanco.Contains(posicao.Id))
            {
                await _context.Custodias
                    .Where(c => c.Id == posicao.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.Quantidade, posicao.Quantidade)
                        .SetProperty(c => c.PrecoMedio, posicao.PrecoMedio)
                        .SetProperty(c => c.DataUltimaAtualizacao, posicao.DataUltimaAtualizacao),
                        cancellationToken);
            }
            else
            {
                await _context.Custodias.AddAsync(posicao, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _context.Entry(posicao).State = EntityState.Detached;
            }
        }
    }
}
