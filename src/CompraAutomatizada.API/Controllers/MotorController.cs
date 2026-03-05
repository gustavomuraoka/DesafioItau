using CompraAutomatizada.Application.UseCases.Motor.ExecutarCompra;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CompraAutomatizada.API.Controllers;

[ApiController]
[Route("api/motor")]
public class MotorController : ControllerBase
{
    private readonly IMediator _mediator;

    public MotorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("executar-compra")]
    public async Task<IActionResult> ExecutarCompra([FromBody] ExecutarCompraRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ExecutarCompraCommand(request.DataReferencia), cancellationToken);
        return Ok(response);
    }
}

public record ExecutarCompraRequest(DateOnly DataReferencia);
