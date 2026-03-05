using CompraAutomatizada.Application.UseCases.Admin.ConsultarCustodiaMaster;
using CompraAutomatizada.Application.UseCases.Cesta.CadastrarCesta;
using CompraAutomatizada.Application.UseCases.Cesta.ConsultarCestaAtiva;
using CompraAutomatizada.Application.UseCases.Cesta.ConsultarHistoricoCestas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CompraAutomatizada.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("cesta")]
    public async Task<IActionResult> CadastrarCesta([FromBody] CadastrarCestaCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return StatusCode(201, response);
    }

    [HttpGet("cesta/atual")]
    public async Task<IActionResult> ConsultarCestaAtiva(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ConsultarCestaAtivaQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("cesta/historico")]
    public async Task<IActionResult> ConsultarHistoricoCestas(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ConsultarHistoricoCestasQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("conta-master/custodia")]
    public async Task<IActionResult> ConsultarCustodiaMaster(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ConsultarCustodiaMasterQuery(), cancellationToken);
        return Ok(response);
    }
}
