using CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class RebalanceamentoRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    private static Rebalanceamento CriarRebalanceamento(long clienteId = 1)
        => Rebalanceamento.Criar(clienteId, TipoRebalanceamento.Desvio, "PETR4", 10, 0, 30m, 5000m, 25m);

    [Fact]
    public async Task AddAsync_DevePersistirRebalanceamento()
    {
        var repo = new RebalanceamentoRepository(_context);
        var rebalanceamento = CriarRebalanceamento();

        await repo.AddAsync(rebalanceamento);
        await _context.SaveChangesAsync();

        var result = await repo.GetByClienteIdAsync(1);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByClienteIdAsync_DeveRetornarApenasDoCliente()
    {
        var repo = new RebalanceamentoRepository(_context);
        await repo.AddAsync(CriarRebalanceamento(clienteId: 1));
        await repo.AddAsync(CriarRebalanceamento(clienteId: 2));
        await _context.SaveChangesAsync();

        var result = await repo.GetByClienteIdAsync(1);

        result.Should().HaveCount(1);
        result.First().ClienteId.Should().Be(1);
    }

    [Fact]
    public async Task GetByClienteIdAsync_ClienteSemRebalanceamentos_DeveRetornarVazio()
    {
        var repo = new RebalanceamentoRepository(_context);

        var result = await repo.GetByClienteIdAsync(999);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByClienteIdAsync_DeveRetornarOrdenadoPorDataDecrescente()
    {
        var repo = new RebalanceamentoRepository(_context);
        await repo.AddAsync(CriarRebalanceamento());
        await _context.SaveChangesAsync();
        await Task.Delay(10);
        await repo.AddAsync(CriarRebalanceamento());
        await _context.SaveChangesAsync();

        var result = (await repo.GetByClienteIdAsync(1)).ToList();

        result.Should().HaveCount(2);
        result[0].DataRebalanceamento.Should().BeOnOrAfter(result[1].DataRebalanceamento);
    }

    [Fact]
    public async Task AddRangeAsync_DevePersistirMultiplos()
    {
        var repo = new RebalanceamentoRepository(_context);
        var lista = new List<Rebalanceamento>
        {
            Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "PETR4", 10, 0, 30m, 5000m, 25m),
            Rebalanceamento.Criar(1, TipoRebalanceamento.MudancaCesta, "VALE3", 0, 10, 80m, 3000m, 70m)
        };

        await repo.AddRangeAsync(lista);
        await _context.SaveChangesAsync();

        var result = await repo.GetByClienteIdAsync(1);
        result.Should().HaveCount(2);
    }

    public void Dispose() => _context.Dispose();
}
