using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.SairDoProduto;

public record SairDoProdutoCommand(long ClienteId) : IRequest<SairDoProdutoResponse>;

public record SairDoProdutoResponse(
    long ClienteId,
    string Nome,
    bool Ativo,
    DateTime DataSaida,
    string Mensagem
);
