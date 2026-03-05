namespace CompraAutomatizada.Application.DTOs;

public record CustodiaMasterDto(
    ContaMasterInfoDto ContaMaster,
    IEnumerable<PosicaoMasterDto> Custodia,
    decimal ValorTotalResiduo
);

public record ContaMasterInfoDto(long Id, string NumeroConta, string Tipo);

public record PosicaoMasterDto(
    string Ticker,
    int Quantidade,
    decimal PrecoMedio,
    decimal ValorAtual,
    string Origem
);
