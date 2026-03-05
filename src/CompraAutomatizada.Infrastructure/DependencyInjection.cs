using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using CompraAutomatizada.Infrastructure.Messaging;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.Infrastructure.Persistence.Repositories;
using CompraAutomatizada.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CompraAutomatizada.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core + MySQL
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Repositories
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IContaGraficaRepository, ContaGraficaRepository>();
        services.AddScoped<ICestaTopFiveRepository, CestaTopFiveRepository>();
        services.AddScoped<ICotacaoRepository, CotacaoRepository>();
        services.AddScoped<IOrdemDeCompraRepository, OrdemDeCompraRepository>();
        services.AddScoped<IEventoIRRepository, EventoIRRepository>();
        services.AddScoped<IRebalanceamentoRepository, RebalanceamentoRepository>();

        // Services
        services.AddScoped<ICotacaoService, CotacaoService>();
        services.AddScoped<ICustodiaService, CustodiaService>();
        services.AddScoped<IIRService, IRService>();
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IRebalanceamentoService, RebalanceamentoService>();

        // Kafka
        services.AddSingleton<IKafkaPublisher, KafkaPublisher>();

        return services;
    }
}
