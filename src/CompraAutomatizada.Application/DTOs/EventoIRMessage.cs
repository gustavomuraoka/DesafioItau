namespace CompraAutomatizada.Application.DTOs;

public record EventoIRMessage(
    long ClienteId,
    string Ticker,
    string Tipo,
    decimal ValorOperacao,
    decimal ValorIr,
    decimal? LucroLiquido,
    DateTime DataEvento
);
