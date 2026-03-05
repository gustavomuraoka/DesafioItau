using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.SairDoProduto;

public class SairDoProdutoHandler : IRequestHandler<SairDoProdutoCommand, SairDoProdutoResponse>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;

    public SairDoProdutoHandler(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository)
    {
        _clienteRepository = clienteRepository;
        _contaGraficaRepository = contaGraficaRepository;
    }

    public async Task<SairDoProdutoResponse> Handle(SairDoProdutoCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken)
            ?? throw new DomainException("Cliente não encontrado.");

        cliente.Sair();

        await _clienteRepository.UpdateAsync(cliente, cancellationToken);

        if (cliente.Conta is not null)
            await _contaGraficaRepository.UpdateAsync(cliente.Conta, cancellationToken);

        return new SairDoProdutoResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Ativo,
            DateTime.UtcNow,
            "Adesão encerrada. Sua posição em custódia foi mantida."
        );
    }
}
