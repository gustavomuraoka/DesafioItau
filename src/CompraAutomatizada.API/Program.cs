using CompraAutomatizada.Application;
using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Infrastructure;
using CompraAutomatizada.Infrastructure.Persistence;
using CompraAutomatizada.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.ContasGraficas.Any(c => c.Tipo == TipoConta.Master))
    {
        var contaMaster = ContaGrafica.CriarMaster();
        db.ContasGraficas.Add(contaMaster);
        db.SaveChanges();
    }

    var cotacaoService = scope.ServiceProvider.GetRequiredService<ICotacaoService>();
    await cotacaoService.ImportarPastaCotacoesAsync();
}


app.Run();
