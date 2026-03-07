using Microsoft.EntityFrameworkCore;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class ContaGraficaRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    [Fact]
    public async Task AddAsync_DevePersistirConta()
    {
        var repo = new ContaGraficaRepository(_context);
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");

        await repo.AddAsync(conta);
        await _context.SaveChangesAsync();

        var salvo = await repo.GetByIdAsync(conta.Id);
        salvo.Should().NotBeNull();
        salvo!.NumeroConta.Should().Be("CLI-000001");
    }

    [Fact]
    public async Task GetByIdAsync_IdInexistente_DeveRetornarNull()
    {
        var repo = new ContaGraficaRepository(_context);

        var result = await repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByClienteIdAsync_DeveRetornarContaDoCliente()
    {
        var repo = new ContaGraficaRepository(_context);
        var conta = ContaGrafica.CriarFilhote(42, "CLI-000042");
        await repo.AddAsync(conta);
        await _context.SaveChangesAsync();

        var result = await repo.GetByClienteIdAsync(42);

        result.Should().NotBeNull();
        result!.NumeroConta.Should().Be("CLI-000042");
    }

    [Fact]
    public async Task GetByClienteIdAsync_ClienteInexistente_DeveRetornarNull()
    {
        var repo = new ContaGraficaRepository(_context);

        var result = await repo.GetByClienteIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMasterAsync_DeveRetornarContaMaster()
    {
        var repo = new ContaGraficaRepository(_context);
        var master = ContaGrafica.CriarMaster();
        await repo.AddAsync(master);
        await _context.SaveChangesAsync();

        var result = await repo.GetMasterAsync();

        result.Should().NotBeNull();
        result!.Tipo.Should().Be(TipoConta.Master);
    }

    [Fact]
    public async Task GetMasterAsync_SemMaster_DeveRetornarNull()
    {
        var repo = new ContaGraficaRepository(_context);

        var result = await repo.GetMasterAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarPosicaoExistente()
    {
        var repo = new ContaGraficaRepository(_context);
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);
        await repo.AddAsync(conta);
        await _context.SaveChangesAsync();

        // recarrega sem AsNoTracking para poder modificar
        var contaTracked = await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .FirstAsync(c => c.Id == conta.Id);

        contaTracked.AdicionarOuAtualizarPosicao("PETR4", 100, 50m);
        await repo.UpdateAsync(contaTracked);

        var atualizada = await repo.GetByIdAsync(conta.Id);
        atualizada!.Posicoes.First().PrecoMedio.Should().Be(40m);
    }

    [Fact]
    public async Task UpdateAsync_DeveAdicionarNovaPosicao()
    {
        var repo = new ContaGraficaRepository(_context);
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        await repo.AddAsync(conta);
        await _context.SaveChangesAsync();

        var contaTracked = await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .FirstAsync(c => c.Id == conta.Id);

        contaTracked.AdicionarOuAtualizarPosicao("VALE3", 50, 80m);
        await repo.UpdateAsync(contaTracked);

        var atualizada = await repo.GetByIdAsync(conta.Id);
        atualizada!.Posicoes.Should().HaveCount(1);
        atualizada.Posicoes.First().Ticker.Should().Be("VALE3");
    }

    [Fact]
    public async Task UpdateAsync_DeveRemoverPosicaoZerada()
    {
        var repo = new ContaGraficaRepository(_context);
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);
        await repo.AddAsync(conta);
        await _context.SaveChangesAsync();

        var contaTracked = await _context.ContasGraficas
            .Include(c => c.Posicoes)
            .FirstAsync(c => c.Id == conta.Id);

        contaTracked.RemoverPosicao("PETR4", 100);
        await repo.UpdateAsync(contaTracked);

        var atualizada = await repo.GetByIdAsync(conta.Id);
        atualizada!.Posicoes.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
