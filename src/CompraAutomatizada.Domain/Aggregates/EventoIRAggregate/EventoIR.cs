using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;

namespace CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;

public class EventoIR : Entity
{
    public long ClienteId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public TipoEventoIR Tipo { get; private set; }
    public decimal ValorOperacao { get; private set; }
    public decimal ValorIr { get; private set; }
    public decimal? LucroLiquido { get; private set; }
    public bool PublicadoKafka { get; private set; }
    public DateTime DataEvento { get; private set; }

    protected EventoIR() { }

    private EventoIR(long clienteId, string ticker, TipoEventoIR tipo, decimal valorOperacao, decimal valorIr, decimal? lucroLiquido)
    {
        ClienteId = clienteId;
        Ticker = ticker.ToUpper();
        Tipo = tipo;
        ValorOperacao = valorOperacao;
        ValorIr = valorIr;
        LucroLiquido = lucroLiquido;
        PublicadoKafka = false;
        DataEvento = DateTime.UtcNow;
    }

    public static EventoIR CriarDedoDuro(long clienteId, string ticker, decimal valorOperacao)
    {
        if (valorOperacao <= 0)
            throw new DomainException("Valor da operação deve ser maior que zero.");

        var valorIr = Math.Round(valorOperacao * 0.00005m, 2);
        return new EventoIR(clienteId, ticker, TipoEventoIR.DedoDuro, valorOperacao, valorIr, null);
    }

    public static EventoIR CriarIrVenda(long clienteId, string ticker, decimal valorVenda, decimal lucroLiquido)
    {
        if (valorVenda <= 0)
            throw new DomainException("Valor da venda deve ser maior que zero.");
        if (lucroLiquido < 0)
            throw new DomainException("Lucro líquido não pode ser negativo para apuração de IR.");

        var valorIr = Math.Round(lucroLiquido * 0.20m, 2);
        return new EventoIR(clienteId, ticker, TipoEventoIR.IrVenda, valorVenda, valorIr, lucroLiquido);
    }

    public void MarcarComoPublicado()
    {
        if (PublicadoKafka)
            throw new DomainException("Evento já foi publicado no Kafka.");

        PublicadoKafka = true;
    }
}
