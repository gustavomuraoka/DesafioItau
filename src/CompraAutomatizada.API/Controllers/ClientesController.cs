using CompraAutomatizada.Application.UseCases.Clientes.AderirAoProduto;
using CompraAutomatizada.Application.UseCases.Clientes.AlterarValorMensal;
using CompraAutomatizada.Application.UseCases.Clientes.ConsultarCarteira;
using CompraAutomatizada.Application.UseCases.Clientes.ConsultarRentabilidade;
using CompraAutomatizada.Application.UseCases.Clientes.SairDoProduto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CompraAutomatizada.API.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("adesao")]
    public async Task<IActionResult> Aderir([FromBody] AderirAoProdutoCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ConsultarCarteira), new { clienteId = response.ClienteId }, response);
    }

    [HttpPost("{clienteId}/saida")]
    public async Task<IActionResult> Sair(long clienteId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new SairDoProdutoCommand(clienteId), cancellationToken);
        return Ok(response);
    }

    [HttpPut("{clienteId}/valor-mensal")]
    public async Task<IActionResult> AlterarValorMensal(long clienteId, [FromBody] AlterarValorMensalRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new AlterarValorMensalCommand(clienteId, request.NovoValorMensal), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{clienteId}/carteira")]
    public async Task<IActionResult> ConsultarCarteira(long clienteId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ConsultarCarteiraQuery(clienteId), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{clienteId}/rentabilidade")]
    public async Task<IActionResult> ConsultarRentabilidade(long clienteId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ConsultarRentabilidadeQuery(clienteId), cancellationToken);
        return Ok(response);
    }
}

public record AlterarValorMensalRequest(decimal NovoValorMensal);
