using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.AlterarValorMensal;

public record AlterarValorMensalCommand(
    long ClienteId,
    decimal NovoValorMensal
) : IRequest<AlterarValorMensalResponse>;

public record AlterarValorMensalResponse(
    long ClienteId,
    decimal ValorMensalAnterior,
    decimal ValorMensalNovo,
    DateTime DataAlteracao,
    string Mensagem
);
