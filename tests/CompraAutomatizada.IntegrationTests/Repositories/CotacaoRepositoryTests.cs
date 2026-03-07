using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class CotacaoRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    private static Cotacao CriarCotacao(string ticker, DateOnly data, decimal fechamento = 30m)
        => Cotacao.Criar(ticker, data, 29m, fechamento, 31m, 28m, 1000);

    [Fact]
    public async Task AddRangeAsync_DevePersistirCotacoes()
    {
        var repo = new CotacaoRepository(_context);
        var cotacoes = new List<Cotacao>
        {
            CriarCotacao("PETR4", new DateOnly(2026, 1, 5)),
            CriarCotacao("VALE3", new DateOnly(2026, 1, 5))
        };

        await repo.AddRangeAsync(cotacoes);
        await repo.SaveChangesAsync();

        var result = await repo.GetUltimaByTickerAsync("PETR4");
        result.Should().NotBeNull();
        result!.Ticker.Should().Be("PETR4");
    }

    [Fact]
    public async Task GetUltimaByTickerAsync_TickerInexistente_DeveRetornarNull()
    {
        var repo = new CotacaoRepository(_context);

        var result = await repo.GetUltimaByTickerAsync("XXXXX");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUltimaByTickerAsync_DeveRetornarMaisRecente()
    {
        var repo = new CotacaoRepository(_context);
        await repo.AddRangeAsync([
            CriarCotacao("PETR4", new DateOnly(2026, 1, 5), 30m),
            CriarCotacao("PETR4", new DateOnly(2026, 1, 10), 35m)
        ]);
        await repo.SaveChangesAsync();

        var result = await repo.GetUltimaByTickerAsync("PETR4");

        result!.PrecoFechamento.Should().Be(35m);
        result.DataPregao.Should().Be(new DateOnly(2026, 1, 10));
    }

    [Fact]
    public async Task GetUltimasByTickersAsync_DeveRetornarUltimaPorTicker()
    {
        var repo = new CotacaoRepository(_context);
        await repo.AddRangeAsync([
            CriarCotacao("PETR4", new DateOnly(2026, 1, 5), 30m),
            CriarCotacao("PETR4", new DateOnly(2026, 1, 10), 35m),
            CriarCotacao("VALE3", new DateOnly(2026, 1, 5), 80m)
        ]);
        await repo.SaveChangesAsync();

        var result = (await repo.GetUltimasByTickersAsync(["PETR4", "VALE3"])).ToList();

        result.Should().HaveCount(2);
        result.First(c => c.Ticker == "PETR4").PrecoFechamento.Should().Be(35m);
    }

    [Fact]
    public async Task GetTickersParaDataAsync_DeveRetornarTickersDaData()
    {
        var repo = new CotacaoRepository(_context);
        await repo.AddRangeAsync([
            CriarCotacao("PETR4", new DateOnly(2026, 1, 5)),
            CriarCotacao("VALE3", new DateOnly(2026, 1, 5)),
            CriarCotacao("ITUB4", new DateOnly(2026, 1, 10))
        ]);
        await repo.SaveChangesAsync();

        var result = await repo.GetTickersParaDataAsync(new DateOnly(2026, 1, 5));

        result.Should().Contain("PETR4");
        result.Should().Contain("VALE3");
        result.Should().NotContain("ITUB4");
    }

    [Fact]
    public async Task GetUltimasByTickersAteDataAsync_DeveIgnorarCotacoesAposData()
    {
        var repo = new CotacaoRepository(_context);
        await repo.AddRangeAsync([
            CriarCotacao("PETR4", new DateOnly(2026, 1, 5), 30m),
            CriarCotacao("PETR4", new DateOnly(2026, 1, 15), 40m)
        ]);
        await repo.SaveChangesAsync();

        var result = (await repo.GetUltimasByTickersAteDataAsync(["PETR4"], new DateOnly(2026, 1, 10))).ToList();

        result.Should().HaveCount(1);
        result[0].PrecoFechamento.Should().Be(30m);
    }

    [Fact]
    public async Task ExisteCotacaoParaDataAsync_CotacaoExistente_DeveRetornarTrue()
    {
        var repo = new CotacaoRepository(_context);
        await repo.AddRangeAsync([CriarCotacao("PETR4", new DateOnly(2026, 1, 5))]);
        await repo.SaveChangesAsync();

        var result = await repo.ExisteCotacaoParaDataAsync("PETR4", new DateOnly(2026, 1, 5));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExisteCotacaoParaDataAsync_CotacaoInexistente_DeveRetornarFalse()
    {
        var repo = new CotacaoRepository(_context);

        var result = await repo.ExisteCotacaoParaDataAsync("PETR4", new DateOnly(2026, 1, 5));

        result.Should().BeFalse();
    }

    public void Dispose() => _context.Dispose();
}
