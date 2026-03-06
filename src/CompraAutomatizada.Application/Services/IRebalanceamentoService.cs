using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;

namespace CompraAutomatizada.Application.Services;

public interface IRebalanceamentoService
{
    Task RebalancearPorMudancaDeCestaAsync(CestaTopFive cestaAntiga, CestaTopFive novaCesta, CancellationToken cancellationToken = default);
    Task RebalancearPorDesvioAsync(IEnumerable<long> clienteIds, CestaTopFive cesta, IReadOnlyDictionary<string, decimal> cotacoes, CancellationToken cancellationToken = default);
}
