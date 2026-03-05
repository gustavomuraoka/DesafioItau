using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Application.Services;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Motor.ExecutarCompra;

public class ExecutarCompraHandler : IRequestHandler<ExecutarCompraCommand, ExecucaoCompraDto>
{
    private readonly ICompraService _compraService;

    public ExecutarCompraHandler(ICompraService compraService)
    {
        _compraService = compraService;
    }

    public async Task<ExecucaoCompraDto> Handle(ExecutarCompraCommand request, CancellationToken cancellationToken)
    {
        return await _compraService.ExecutarAsync(request.DataReferencia, cancellationToken);
    }
}
