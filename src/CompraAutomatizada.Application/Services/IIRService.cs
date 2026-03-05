namespace CompraAutomatizada.Application.Services;

public interface IIRService
{
    Task ProcessarDedoDuroAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken cancellationToken = default);
    Task ProcessarIRVendaAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, decimal precoMedioAquisicao, CancellationToken cancellationToken = default);
}
