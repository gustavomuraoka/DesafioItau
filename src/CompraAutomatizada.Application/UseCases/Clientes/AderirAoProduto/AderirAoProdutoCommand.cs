using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.AderirAoProduto;

public record AderirAoProdutoCommand(
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal
) : IRequest<AderirAoProdutoResponse>;

public record AderirAoProdutoResponse(
    long ClienteId,
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal,
    bool Ativo,
    DateTime DataAdesao,
    ContaGraficaResponse ContaGrafica
);

public record ContaGraficaResponse(
    long Id,
    string NumeroConta,
    string Tipo,
    DateTime DataCriacao
);
