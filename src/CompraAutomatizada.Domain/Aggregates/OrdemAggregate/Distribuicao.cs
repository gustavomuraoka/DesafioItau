using CompraAutomatizada.Domain.Common;

namespace CompraAutomatizada.Domain.Aggregates.OrdemAggregate;

public class Distribuicao : Entity
{
    public long OrdemCompraId { get; private set; }
    public long CustodiaFilhoteId { get; private set; }
    public long ClienteId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal ValorIrDedoDuro { get; private set; }
    public DateTime DataDistribuicao { get; private set; }

    public decimal ValorTotal => Quantidade * PrecoUnitario;

    protected Distribuicao() { }

    private Distribuicao(long ordemCompraId, long custodiaFilhoteId, long clienteId, string ticker, int quantidade, decimal precoUnitario, DateTime dataDistribuicao)
    {
        OrdemCompraId = ordemCompraId;
        CustodiaFilhoteId = custodiaFilhoteId;
        ClienteId = clienteId;
        Ticker = ticker.ToUpper();
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        ValorIrDedoDuro = CalcularIrDedoDuro(quantidade * precoUnitario);
        DataDistribuicao = dataDistribuicao;
    }

    internal static Distribuicao Criar(long ordemCompraId, long custodiaFilhoteId, long clienteId, string ticker, int quantidade, decimal precoUnitario, DateTime dataDistribuicao)
    {
        if (ordemCompraId <= 0) throw new DomainException("OrdemCompraId inválido.");
        if (custodiaFilhoteId <= 0) throw new DomainException("CustodiaFilhoteId inválido.");
        if (clienteId <= 0) throw new DomainException("ClienteId inválido.");
        if (string.IsNullOrWhiteSpace(ticker)) throw new DomainException("Ticker é obrigatório.");
        if (quantidade <= 0) throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0) throw new DomainException("Preço unitário deve ser maior que zero.");

        return new Distribuicao(ordemCompraId, custodiaFilhoteId, clienteId, ticker, quantidade, precoUnitario, dataDistribuicao);
    }

    private static decimal CalcularIrDedoDuro(decimal valorTotal)
        => Math.Round(valorTotal * 0.00005m, 2);
}
