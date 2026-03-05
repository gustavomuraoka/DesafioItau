using CompraAutomatizada.Domain.Common;

namespace CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;

public class Cotacao : Entity
{
    public string Ticker { get; private set; } = string.Empty;
    public DateOnly DataPregao { get; private set; }
    public decimal PrecoAbertura { get; private set; }
    public decimal PrecoFechamento { get; private set; }
    public decimal PrecoMaximo { get; private set; }
    public decimal PrecoMinimo { get; private set; }
    public long VolumeNegociado { get; private set; }

    protected Cotacao() { }

    private Cotacao(string ticker, DateOnly dataPregao, decimal precoAbertura, decimal precoFechamento, decimal precoMaximo, decimal precoMinimo, long volumeNegociado)
    {
        Ticker = ticker.ToUpper();
        DataPregao = dataPregao;
        PrecoAbertura = precoAbertura;
        PrecoFechamento = precoFechamento;
        PrecoMaximo = precoMaximo;
        PrecoMinimo = precoMinimo;
        VolumeNegociado = volumeNegociado;
    }

    public static Cotacao Criar(string ticker, DateOnly dataPregao, decimal precoAbertura, decimal precoFechamento, decimal precoMaximo, decimal precoMinimo, long volumeNegociado)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainException("Ticker é obrigatório.");
        if (precoFechamento <= 0)
            throw new DomainException("Preço de fechamento deve ser maior que zero.");
        if (precoMaximo < precoMinimo)
            throw new DomainException("Preço máximo não pode ser menor que o preço mínimo.");

        return new Cotacao(ticker, dataPregao, precoAbertura, precoFechamento, precoMaximo, precoMinimo, volumeNegociado);
    }
}
