namespace CompraAutomatizada.Application.DTOs;

public record CestaDto(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    DateTime? DataDesativacao,
    IEnumerable<ItemCestaDto> Itens
);

public record ItemCestaDto(string Ticker, decimal Percentual);

public record ItemCestaComCotacaoDto(string Ticker, decimal Percentual, decimal CotacaoAtual);

public record CestaAtualDto(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    IEnumerable<ItemCestaComCotacaoDto> Itens
);

public record HistoricoCestasDto(IEnumerable<CestaDto> Cestas);
