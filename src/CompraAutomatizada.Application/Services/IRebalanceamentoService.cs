using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;

namespace CompraAutomatizada.Application.Services;

public interface IRebalanceamentoService
{
    Task RebalancearPorMudancaDeCestaAsync(CestaTopFive cestaAntiga, CestaTopFive novaCesta, CancellationToken cancellationToken = default);
    Task RebalancearPorDesvioAsync(CancellationToken cancellationToken = default);
}
