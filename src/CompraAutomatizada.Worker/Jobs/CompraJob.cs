using CompraAutomatizada.Application.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CompraAutomatizada.Worker.Jobs;

[DisallowConcurrentExecution]
public class CompraJob : IJob
{
    private readonly ICompraService _compraService;
    private readonly ILogger<CompraJob> _logger;

    public CompraJob(ICompraService compraService, ILogger<CompraJob> logger)
    {
        _compraService = compraService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);

        if (!EhDiaDeCompra(hoje))
        {
            _logger.LogInformation("Data {Data} não é dia de compra. Job encerrado.", hoje);
            return;
        }

        _logger.LogInformation("Iniciando compra programada automática para {Data}.", hoje);

        try
        {
            var resultado = await _compraService.ExecutarAsync(hoje, context.CancellationToken);
            _logger.LogInformation("Compra automática concluída. Clientes: {N}, Total: {Total}",
                resultado.TotalClientes, resultado.TotalConsolidado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na execução automática da compra programada.");
            throw;
        }
    }

    private static bool EhDiaDeCompra(DateOnly data)
    {
        if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
            return false;

        foreach (var diaAlvo in new[] { 5, 15, 25 })
        {
            var dataAlvo = new DateOnly(data.Year, data.Month, diaAlvo);

            while (dataAlvo.DayOfWeek == DayOfWeek.Saturday || dataAlvo.DayOfWeek == DayOfWeek.Sunday)
                dataAlvo = dataAlvo.AddDays(1);

            if (data == dataAlvo)
                return true;
        }

        return false;
    }
}
