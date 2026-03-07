using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class OrdemDeCompraRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    private static OrdemDeCompra CriarOrdem()
        => OrdemDeCompra.Criar(1, "PETR4", 100, 30m, TipoMercado.Lote);

    [Fact]
    public async Task AddAsync_DevePersistirOrdem()
    {
        var repo = new OrdemDeCompraRepository(_context);
        var ordem = CriarOrdem();

        await repo.AddAsync(ordem);
        await repo.SaveChangesAsync();

        var salvo = await repo.GetByIdAsync(ordem.Id);
        salvo.Should().NotBeNull();
        salvo!.Ticker.Should().Be("PETR4");
    }

    [Fact]
    public async Task GetByIdAsync_IdInexistente_DeveRetornarNull()
    {
        var repo = new OrdemDeCompraRepository(_context);

        var result = await repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeveCarregarDistribuicoes()
    {
        var repo = new OrdemDeCompraRepository(_context);
        var ordem = CriarOrdem();
        await repo.AddAsync(ordem);
        await repo.SaveChangesAsync();

        ordem.AdicionarDistribuicao(1, 1, 50, 30m, DateTime.UtcNow);
        await repo.SaveChangesAsync();

        var result = await repo.GetByIdAsync(ordem.Id);
        result!.Distribuicoes.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRangeAsync_DevePersistirMultiplasOrdens()
    {
        var repo = new OrdemDeCompraRepository(_context);
        var ordens = new List<OrdemDeCompra>
        {
            OrdemDeCompra.Criar(1, "PETR4", 100, 30m, TipoMercado.Lote),
            OrdemDeCompra.Criar(1, "PETR4", 50, 30m, TipoMercado.Fracionario)
        };

        await repo.AddRangeAsync(ordens);
        await repo.SaveChangesAsync();

        var salvo1 = await repo.GetByIdAsync(ordens[0].Id);
        var salvo2 = await repo.GetByIdAsync(ordens[1].Id);
        salvo1.Should().NotBeNull();
        salvo2.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDistribuicoesByClienteIdAsync_DeveRetornarOrdenadas()
    {
        var repo = new OrdemDeCompraRepository(_context);
        var ordem = CriarOrdem();
        await repo.AddAsync(ordem);
        await repo.SaveChangesAsync();

        var data1 = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var data2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        ordem.AdicionarDistribuicao(1, 1, 50, 30m, data2);
        ordem.AdicionarDistribuicao(1, 1, 50, 30m, data1);
        await repo.SaveChangesAsync();

        var result = (await repo.GetDistribuicoesByClienteIdAsync(1)).ToList();

        result.Should().HaveCount(2);
        result[0].DataDistribuicao.Should().BeBefore(result[1].DataDistribuicao);
    }

    [Fact]
    public async Task GetDistribuicoesByClienteIdAsync_ClienteSemDistribuicoes_DeveRetornarVazio()
    {
        var repo = new OrdemDeCompraRepository(_context);

        var result = await repo.GetDistribuicoesByClienteIdAsync(999);

        result.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
