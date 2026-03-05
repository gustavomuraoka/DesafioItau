using CompraAutomatizada.Application.DTOs;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Motor.ExecutarCompra;

public record ExecutarCompraCommand(DateOnly DataReferencia) : IRequest<ExecucaoCompraDto>;
