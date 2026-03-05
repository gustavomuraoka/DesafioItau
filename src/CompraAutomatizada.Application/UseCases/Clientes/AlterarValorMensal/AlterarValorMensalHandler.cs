using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.AlterarValorMensal;

public class AlterarValorMensalHandler : IRequestHandler<AlterarValorMensalCommand, AlterarValorMensalResponse>
{
    private readonly IClienteRepository _clienteRepository;

    public AlterarValorMensalHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<AlterarValorMensalResponse> Handle(AlterarValorMensalCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken)
            ?? throw new DomainException("Cliente não encontrado.");

        var valorAnterior = cliente.ValorMensal;

        cliente.AlterarValorMensal(request.NovoValorMensal);

        await _clienteRepository.UpdateAsync(cliente, cancellationToken);

        return new AlterarValorMensalResponse(
            cliente.Id,
            valorAnterior,
            cliente.ValorMensal,
            DateTime.UtcNow,
            "Valor mensal atualizado. O novo valor será considerado a partir da próxima data de compra."
        );
    }
}
