namespace CompraAutomatizada.Application.DTOs;

public record ExecucaoCompraDto(
    DateTime DataExecucao,
    int TotalClientes,
    decimal TotalConsolidado,
    IEnumerable<OrdemCompraDto> OrdensCompra,
    IEnumerable<DistribuicaoClienteDto> Distribuicoes,
    IEnumerable<ResiduoDto> ResiduosCustMaster,
    int EventosIRPublicados,
    string Mensagem
);

public record OrdemCompraDto(
    string Ticker,
    int QuantidadeTotal,
    IEnumerable<DetalheOrdemDto> Detalhes,
    decimal PrecoUnitario,
    decimal ValorTotal
);

public record DetalheOrdemDto(string Tipo, string Ticker, int Quantidade);

public record DistribuicaoClienteDto(
    long ClienteId,
    string Nome,
    decimal ValorAporte,
    IEnumerable<AtivoDistribuidoDto> Ativos
);

public record AtivoDistribuidoDto(string Ticker, int Quantidade);

public record ResiduoDto(string Ticker, int Quantidade);
