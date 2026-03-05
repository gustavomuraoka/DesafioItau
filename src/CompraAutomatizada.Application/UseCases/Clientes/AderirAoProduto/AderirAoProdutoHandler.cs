using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.AderirAoProduto;

public class AderirAoProdutoHandler : IRequestHandler<AderirAoProdutoCommand, AderirAoProdutoResponse>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;

    public AderirAoProdutoHandler(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository)
    {
        _clienteRepository = clienteRepository;
        _contaGraficaRepository = contaGraficaRepository;
    }

    public async Task<AderirAoProdutoResponse> Handle(AderirAoProdutoCommand request, CancellationToken cancellationToken)
    {
        var clienteExistente = await _clienteRepository.GetByCpfAsync(request.Cpf, cancellationToken);
        if (clienteExistente is not null)
            throw new DomainException("CPF já cadastrado no sistema.");

        var cliente = Cliente.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal);
        await _clienteRepository.AddAsync(cliente, cancellationToken);
        await _clienteRepository.SaveChangesAsync(cancellationToken);

        var numeroConta = $"FLH-{cliente.Id:D6}";
        var conta = cliente.AbrirConta(numeroConta);
        await _contaGraficaRepository.AddAsync(conta, cancellationToken);
        await _clienteRepository.UpdateAsync(cliente, cancellationToken);
        await _clienteRepository.SaveChangesAsync(cancellationToken);

        return new AderirAoProdutoResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Cpf.Valor,
            cliente.Email,
            cliente.ValorMensal,
            cliente.Ativo,
            cliente.DataAdesao,
            new ContaGraficaResponse(
                conta.Id,
                conta.NumeroConta,
                conta.Tipo.ToString().ToUpper(),
                conta.DataCriacao
            )
        );
    }

}
