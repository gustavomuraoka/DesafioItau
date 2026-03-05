using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.ValueObjects;

namespace CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;

public class ItemCesta : Entity
{
    public long CestaId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public Proporcao Percentual { get; private set; } = null!;

    protected ItemCesta() { }

    private ItemCesta(long cestaId, string ticker, decimal percentual)
    {
        CestaId = cestaId;
        Ticker = ticker.ToUpper();
        Percentual = new Proporcao(percentual);
    }

    internal static ItemCesta Criar(long cestaId, string ticker, decimal percentual)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainException("Ticker é obrigatório.");

        return new ItemCesta(cestaId, ticker, percentual);
    }
}
