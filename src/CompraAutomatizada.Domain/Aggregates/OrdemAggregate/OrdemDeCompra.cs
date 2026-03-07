using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;

namespace CompraAutomatizada.Domain.Aggregates.OrdemAggregate;

public class OrdemDeCompra : Entity
{
    public long ContaMasterId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public TipoMercado TipoMercado { get; private set; }
    public DateTime DataExecucao { get; private set; }

    public decimal ValorTotal => Quantidade * PrecoUnitario;

    private readonly List<Distribuicao> _distribuicoes = new();
    public IReadOnlyCollection<Distribuicao> Distribuicoes => _distribuicoes.AsReadOnly();

    protected OrdemDeCompra() { }

    private OrdemDeCompra(long contaMasterId, string ticker, int quantidade, decimal precoUnitario, TipoMercado tipoMercado)
    {
        ContaMasterId = contaMasterId;
        Ticker = ticker.ToUpper();
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        TipoMercado = tipoMercado;
        DataExecucao = DateTime.UtcNow;
    }

    public static OrdemDeCompra Criar(long contaMasterId, string ticker, int quantidade, decimal precoUnitario, TipoMercado tipoMercado)
    {
        if (contaMasterId <= 0)
            throw new DomainException("ContaMasterId inv�lido.");
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainException("Ticker � obrigat�rio.");
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new DomainException("Pre�o unit�rio deve ser maior que zero.");
        if (tipoMercado == TipoMercado.Lote && quantidade % 100 != 0)
            throw new DomainException("Ordens de lote padr�o devem ter quantidade m�ltipla de 100.");
        if (tipoMercado == TipoMercado.Fracionario && quantidade >= 100)
            throw new DomainException("Ordens fracion�rias devem ter quantidade menor que 100.");

        return new OrdemDeCompra(contaMasterId, ticker, quantidade, precoUnitario, tipoMercado);
    }

    public static IEnumerable<OrdemDeCompra> CriarComSplit(long contaMasterId, string ticker, int quantidadeTotal, decimal precoUnitario)
    {
        if (quantidadeTotal <= 0)
            throw new DomainException("Quantidade total deve ser maior que zero.");

        var ordens = new List<OrdemDeCompra>();

        var lotes = (quantidadeTotal / 100) * 100;
        var fracionario = quantidadeTotal % 100;

        if (lotes > 0)
            ordens.Add(Criar(contaMasterId, ticker, lotes, precoUnitario, TipoMercado.Lote));

        if (fracionario > 0)
            ordens.Add(Criar(contaMasterId, $"{ticker}F", fracionario, precoUnitario, TipoMercado.Fracionario));

        return ordens;
    }

    public Distribuicao AdicionarDistribuicao(long custodiaFilhoteId, long clienteId, int quantidade, decimal precoUnitario, DateTime dataDistribuicao)
    {
        var distribuicao = Distribuicao.Criar(Id, custodiaFilhoteId, clienteId, Ticker, quantidade, precoUnitario, dataDistribuicao);
        _distribuicoes.Add(distribuicao);
        return distribuicao;
    }


}
