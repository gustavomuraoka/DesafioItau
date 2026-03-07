using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.IntegrationTests.Helpers;
using FluentAssertions;

namespace CompraAutomatizada.IntegrationTests.Repositories;

public class ClienteRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = DbContextFactory.Create();

    [Fact]
    public async Task AddAsync_DevePersistitrCliente()
    {
        var repo = new ClienteRepository(_context);
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);

        await repo.AddAsync(cliente);
        await repo.SaveChangesAsync();

        var salvo = await repo.GetByIdAsync(cliente.Id);
        salvo.Should().NotBeNull();
        salvo!.Nome.Should().Be("Gustavo");
    }

    [Fact]
    public async Task GetByIdAsync_IdInexistente_DeveRetornarNull()
    {
        var repo = new ClienteRepository(_context);

        var result = await repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCpfAsync_DeveRetornarCliente()
    {
        var repo = new ClienteRepository(_context);
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);
        await repo.AddAsync(cliente);
        await repo.SaveChangesAsync();

        var result = await repo.GetByCpfAsync("52998224725");

        result.Should().NotBeNull();
    }

    public void Dispose() => _context.Dispose();
}
