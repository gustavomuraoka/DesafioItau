using CompraAutomatizada.Application.DTOs;

namespace CompraAutomatizada.Application.Services;

public interface ICompraService
{
    Task<ExecucaoCompraDto> ExecutarAsync(DateOnly dataReferencia, CancellationToken cancellationToken = default);
}
