using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class EventoIRRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    [Fact]
    public async Task AddAsync_DevePersistirEvento()
    {
        var repo = new EventoIRRepository(_context);
        var evento = EventoIR.CriarDedoDuro(1, "PETR4", 10000m);

        await repo.AddAsync(evento);
        await repo.SaveChangesAsync();

        var naoPublicados = await repo.GetNaoPublicadosAsync();
        naoPublicados.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetNaoPublicadosAsync_DeveRetornarApenasNaoPublicados()
    {
        var repo = new EventoIRRepository(_context);
        var publicado = EventoIR.CriarDedoDuro(1, "PETR4", 10000m);
        var naoPublicado = EventoIR.CriarDedoDuro(1, "VALE3", 5000m);

        await repo.AddAsync(publicado);
        await repo.AddAsync(naoPublicado);
        await repo.SaveChangesAsync();

        publicado.MarcarComoPublicado();
        await repo.UpdateAsync(publicado);
        await repo.SaveChangesAsync();

        var result = (await repo.GetNaoPublicadosAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Ticker.Should().Be("VALE3");
    }

    [Fact]
    public async Task GetTotalVendasNoMesAsync_DeveSomarApenasVendasDoMes()
    {
        var repo = new EventoIRRepository(_context);
        var venda1 = EventoIR.CriarIrVenda(1, "PETR4", 25000m, 1000m);
        var venda2 = EventoIR.CriarIrVenda(1, "VALE3", 10000m, 500m);

        await repo.AddAsync(venda1);
        await repo.AddAsync(venda2);
        await repo.SaveChangesAsync();

        var total = await repo.GetTotalVendasNoMesAsync(1, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

        total.Should().Be(35000m);
    }

    [Fact]
    public async Task GetTotalVendasNoMesAsync_SemVendas_DeveRetornarZero()
    {
        var repo = new EventoIRRepository(_context);

        var total = await repo.GetTotalVendasNoMesAsync(1, 2026, 1);

        total.Should().Be(0m);
    }

    [Fact]
    public async Task UpdateAsync_DeveMarcarComoPublicado()
    {
        var repo = new EventoIRRepository(_context);
        var evento = EventoIR.CriarDedoDuro(1, "PETR4", 10000m);
        await repo.AddAsync(evento);
        await repo.SaveChangesAsync();

        evento.MarcarComoPublicado();
        await repo.UpdateAsync(evento);
        await repo.SaveChangesAsync();

        var naoPublicados = await repo.GetNaoPublicadosAsync();
        naoPublicados.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
