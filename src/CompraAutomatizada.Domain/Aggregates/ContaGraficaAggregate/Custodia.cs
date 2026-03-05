using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;

namespace CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;

public class Custodia : Entity
{
    public long ContaGraficaId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoMedio { get; private set; }
    public DateTime DataUltimaAtualizacao { get; private set; }

    private TipoConta _tipoConta;

    protected Custodia() { }

    private Custodia(long contaGraficaId, string ticker, int quantidade, decimal precoCompra, TipoConta tipoConta)
    {
        ContaGraficaId = contaGraficaId;
        Ticker = ticker.ToUpper();
        Quantidade = quantidade;
        PrecoMedio = precoCompra;
        DataUltimaAtualizacao = DateTime.UtcNow;
        _tipoConta = tipoConta;
    }

    internal static Custodia Criar(long contaGraficaId, string ticker, int quantidade, decimal precoCompra, TipoConta tipoConta)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new DomainException("Ticker é obrigatório.");
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoCompra <= 0)
            throw new DomainException("Preço de compra deve ser maior que zero.");

        return new Custodia(contaGraficaId, ticker, quantidade, precoCompra, tipoConta);
    }

    internal void AtualizarAposCompra(int quantidadeNova, decimal precoNovaCompra)
    {
        if (quantidadeNova <= 0)
            throw new DomainException("Quantidade nova deve ser maior que zero.");
        if (precoNovaCompra <= 0)
            throw new DomainException("Preço de compra deve ser maior que zero.");

        PrecoMedio = (Quantidade * PrecoMedio + quantidadeNova * precoNovaCompra)
                     / (Quantidade + quantidadeNova);
        Quantidade += quantidadeNova;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    internal void RemoverQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade a remover deve ser maior que zero.");
        if (quantidade > Quantidade)
            throw new DomainException($"Saldo insuficiente para {Ticker}. Disponível: {Quantidade}, solicitado: {quantidade}.");

        Quantidade -= quantidade;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    internal void DistribuirParaFilhote(int quantidade)
    {
        if (_tipoConta != TipoConta.Master)
            throw new DomainException("Apenas custódias da conta master podem realizar distribuições.");

        RemoverQuantidade(quantidade);
    }
}
