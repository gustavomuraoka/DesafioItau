namespace CompraAutomatizada.Application.DTOs;

public record RentabilidadeDto(
    long ClienteId,
    string Nome,
    DateTime DataConsulta,
    ResumoRentabilidadeDto Rentabilidade,
    IEnumerable<HistoricoAporteDto> HistoricoAportes,
    IEnumerable<EvolucaoCarteiraDto> EvolucaoCarteira
);

public record ResumoRentabilidadeDto(
    decimal ValorTotalInvestido,
    decimal ValorAtualCarteira,
    decimal PlTotal,
    decimal RentabilidadePercentual
);

public record HistoricoAporteDto(
    DateOnly Data,
    decimal Valor,
    string Parcela
);

public record EvolucaoCarteiraDto(
    DateOnly Data,
    decimal ValorCarteira,
    decimal ValorInvestido,
    decimal Rentabilidade
);
