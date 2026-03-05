using MediatR;

namespace CompraAutomatizada.Application.UseCases.Cesta.CadastrarCesta;

public record CadastrarCestaCommand(
    string Nome,
    IEnumerable<ItemCestaInput> Itens
) : IRequest<CadastrarCestaResponse>;

public record ItemCestaInput(string Ticker, decimal Percentual);

public record CadastrarCestaResponse(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    IEnumerable<ItemCestaInput> Itens,
    bool RebalanceamentoDisparado,
    CestaAnteriorInfo? CestaAnteriorDesativada,
    IEnumerable<string>? AtivosRemovidos,
    IEnumerable<string>? AtivosAdicionados,
    string Mensagem
);

public record CestaAnteriorInfo(long CestaId, string Nome, DateTime DataDesativacao);
