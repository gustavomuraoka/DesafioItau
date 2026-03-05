namespace CompraAutomatizada.Application.Services;

public interface ICustodiaService
{
	Task AdicionarNaMasterAsync(string ticker, int quantidade, decimal precoUnitario, CancellationToken cancellationToken = default);
	Task DistribuirParaFilhoteAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken cancellationToken = default);
	Task DescontarDaMasterAsync(string ticker, int quantidade, CancellationToken cancellationToken = default);
	Task<int> ObterSaldoMasterAsync(string ticker, CancellationToken cancellationToken = default);
	Task<decimal> VenderDaFilhoteAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, CancellationToken cancellationToken = default);
}
