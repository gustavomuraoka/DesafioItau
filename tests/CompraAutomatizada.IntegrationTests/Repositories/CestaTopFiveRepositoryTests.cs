using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class CestaTopFiveRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    private static List<(string Ticker, decimal Percentual)> ItensValidos() =>
    [
        ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("WEGE3", 20m)
    ];

    [Fact]
    public async Task AddAsync_DevePersistirCesta()
    {
        var repo = new CestaTopFiveRepository(_context);
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());

        await repo.AddAsync(cesta);
        await repo.SaveChangesAsync();

        var ativa = await repo.GetAtivaAsync();
        ativa.Should().NotBeNull();
        ativa!.Nome.Should().Be("Top 5");
    }

    [Fact]
    public async Task GetAtivaAsync_SemCestaAtiva_DeveRetornarNull()
    {
        var repo = new CestaTopFiveRepository(_context);

        var result = await repo.GetAtivaAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAtivaAsync_CestaDesativada_DeveRetornarNull()
    {
        var repo = new CestaTopFiveRepository(_context);
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());
        await repo.AddAsync(cesta);
        await repo.SaveChangesAsync();

        cesta.Desativar();
        await repo.UpdateAsync(cesta);
        await repo.SaveChangesAsync();

        var result = await repo.GetAtivaAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAtivaAsync_DeveCarregarItens()
    {
        var repo = new CestaTopFiveRepository(_context);
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());
        await repo.AddAsync(cesta);
        await repo.SaveChangesAsync();

        var result = await repo.GetAtivaAsync();

        result!.Itens.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetHistoricoAsync_DeveRetornarOrdenadoPorDataDecrescente()
    {
        var repo = new CestaTopFiveRepository(_context);

        var cesta1 = CestaTopFive.Criar("Cesta 1", ItensValidos());
        await repo.AddAsync(cesta1);
        await repo.SaveChangesAsync();

        await Task.Delay(10); // garante DataCriacao diferente

        var cesta2 = CestaTopFive.Criar("Cesta 2",
        [
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("RENT3", 20m)
        ]);
        await repo.AddAsync(cesta2);
        await repo.SaveChangesAsync();

        var historico = (await repo.GetHistoricoAsync()).ToList();

        historico.Should().HaveCount(2);
        historico[0].Nome.Should().Be("Cesta 2");
    }

    [Fact]
    public async Task UpdateAsync_DeveDesativarCesta()
    {
        var repo = new CestaTopFiveRepository(_context);
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());
        await repo.AddAsync(cesta);
        await repo.SaveChangesAsync();

        cesta.Desativar();
        await repo.UpdateAsync(cesta);
        await repo.SaveChangesAsync();

        var result = await repo.GetAtivaAsync();
        result.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
