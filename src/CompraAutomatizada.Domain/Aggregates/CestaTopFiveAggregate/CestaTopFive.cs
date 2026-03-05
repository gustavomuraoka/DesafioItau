using CompraAutomatizada.Domain.Common;

namespace CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;

public class CestaTopFive : Entity
{
    public const int TotalDeAtivos = 5;
    public const decimal SomaPercentuais = 100m;

    public string Nome { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataDesativacao { get; private set; }

    private readonly List<ItemCesta> _itens = new();
    public IReadOnlyCollection<ItemCesta> Itens => _itens.AsReadOnly();

    protected CestaTopFive() { }

    private CestaTopFive(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da cesta é obrigatório.");

        Nome = nome;
        Ativa = true;
        DataCriacao = DateTime.UtcNow;
    }

    public static CestaTopFive Criar(string nome, IEnumerable<(string Ticker, decimal Percentual)> itens)
    {
        var lista = itens.ToList();

        if (lista.Count != TotalDeAtivos)
            throw new DomainException($"A cesta deve conter exatamente {TotalDeAtivos} ativos.");

        var tickers = lista.Select(i => i.Ticker.ToUpper()).ToList();
        if (tickers.Distinct().Count() != tickers.Count)
            throw new DomainException("A cesta não pode conter tickers duplicados.");

        var somaPercentuais = lista.Sum(i => i.Percentual);
        if (somaPercentuais != SomaPercentuais)
            throw new DomainException($"A soma dos percentuais deve ser exatamente {SomaPercentuais}%. Soma atual: {somaPercentuais}%.");

        var cesta = new CestaTopFive(nome);

        foreach (var (ticker, percentual) in lista)
            cesta._itens.Add(ItemCesta.Criar(0, ticker, percentual));

        return cesta;
    }

    public void Desativar()
    {
        if (!Ativa)
            throw new DomainException("Cesta já está inativa.");

        Ativa = false;
        DataDesativacao = DateTime.UtcNow;
    }
}
