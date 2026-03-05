using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;

namespace CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;

public class Rebalanceamento : Entity
{
    public long ClienteId { get; private set; }
    public TipoRebalanceamento Tipo { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int QuantidadeVendida { get; private set; }
    public int QuantidadeComprada { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal ValorTotalVendas { get; private set; }
    public decimal? ValorIrApurado { get; private set; }
    public DateTime DataRebalanceamento { get; private set; }

    public decimal ValorOperacao => (QuantidadeVendida + QuantidadeComprada) * PrecoUnitario;

    protected Rebalanceamento() { }

    private Rebalanceamento(long clienteId, TipoRebalanceamento tipo, string ticker, int quantidadeVendida, int quantidadeComprada, decimal precoUnitario, decimal valorTotalVendas, decimal? valorIrApurado)
    {
        ClienteId = clienteId;
        Tipo = tipo;
        Ticker = ticker.ToUpper();
        QuantidadeVendida = quantidadeVendida;
        QuantidadeComprada = quantidadeComprada;
        PrecoUnitario = precoUnitario;
        ValorTotalVendas = valorTotalVendas;
        ValorIrApurado = valorIrApurado;
        DataRebalanceamento = DateTime.UtcNow;
    }

    public static Rebalanceamento Criar(long clienteId, TipoRebalanceamento tipo, string ticker, int quantidadeVendida, int quantidadeComprada, decimal precoUnitario, decimal valorTotalVendasNoMes, decimal precoMedioAquisicao)
    {
        if (clienteId <= 0)
            throw new DomainException("ClienteId inválido.");
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainException("Ticker é obrigatório.");
        if (quantidadeVendida < 0)
            throw new DomainException("Quantidade vendida não pode ser negativa.");
        if (quantidadeComprada < 0)
            throw new DomainException("Quantidade comprada não pode ser negativa.");
        if (quantidadeVendida == 0 && quantidadeComprada == 0)
            throw new DomainException("Rebalanceamento deve ter ao menos uma operação.");
        if (precoUnitario <= 0)
            throw new DomainException("Preço unitário deve ser maior que zero.");

        decimal? valorIrApurado = null;

        if (quantidadeVendida > 0 && valorTotalVendasNoMes > 20_000m)
        {
            var lucroLiquido = (precoUnitario - precoMedioAquisicao) * quantidadeVendida;
            if (lucroLiquido > 0)
                valorIrApurado = Math.Round(lucroLiquido * 0.20m, 2);
        }

        return new Rebalanceamento(clienteId, tipo, ticker, quantidadeVendida, quantidadeComprada, precoUnitario, valorTotalVendasNoMes, valorIrApurado);
    }
}
