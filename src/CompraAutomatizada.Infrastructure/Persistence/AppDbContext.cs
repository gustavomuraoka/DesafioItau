using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;
using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;
using Microsoft.EntityFrameworkCore;

namespace CompraAutomatizada.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ContaGrafica> ContasGraficas => Set<ContaGrafica>();
    public DbSet<Custodia> Custodias => Set<Custodia>();
    public DbSet<CestaTopFive> CestasRecomendacao => Set<CestaTopFive>();
    public DbSet<ItemCesta> ItensCesta => Set<ItemCesta>();
    public DbSet<OrdemDeCompra> OrdensCompra => Set<OrdemDeCompra>();
    public DbSet<Distribuicao> Distribuicoes => Set<Distribuicao>();
    public DbSet<EventoIR> EventosIR => Set<EventoIR>();
    public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
    public DbSet<Rebalanceamento> Rebalanceamentos => Set<Rebalanceamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
