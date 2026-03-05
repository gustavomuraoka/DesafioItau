using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;

namespace CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;

public class ContaGrafica : Entity
{
    public const long IdContaMaster = 1;
    public const string NumeroContaMaster = "MST-000001";

    public long? ClienteId { get; private set; }
    public string NumeroConta { get; private set; } = string.Empty;
    public TipoConta Tipo { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private readonly List<Custodia> _posicoes = new();
    public IReadOnlyCollection<Custodia> Posicoes => _posicoes.AsReadOnly();

    protected ContaGrafica() { }

    private ContaGrafica(long? clienteId, string numeroConta, TipoConta tipo)
    {
        ClienteId = clienteId;
        NumeroConta = numeroConta;
        Tipo = tipo;
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
    }

    public static ContaGrafica CriarMaster()
    {
        var conta = new ContaGrafica(null, NumeroContaMaster, TipoConta.Master);
        conta.Id = IdContaMaster;
        return conta;
    }

    public static ContaGrafica CriarFilhote(long clienteId, string numeroConta)
    {
        if (clienteId <= 0)
            throw new DomainException("ClienteId inválido.");
        if (string.IsNullOrWhiteSpace(numeroConta))
            throw new DomainException("Número de conta é obrigatório.");

        return new ContaGrafica(clienteId, numeroConta, TipoConta.Filhote);
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Conta já está inativa.");
        Ativo = false;
    }

    public void AdicionarOuAtualizarPosicao(string ticker, int quantidade, decimal precoCompra)
    {
        var posicao = _posicoes.FirstOrDefault(c => c.Ticker == ticker.ToUpper());

        if (posicao is null)
            _posicoes.Add(Custodia.Criar(Id, ticker, quantidade, precoCompra, Tipo));
        else
            posicao.AtualizarAposCompra(quantidade, precoCompra);
    }

    public void RemoverPosicao(string ticker, int quantidade)
    {
        var posicao = _posicoes.FirstOrDefault(c => c.Ticker == ticker.ToUpper())
            ?? throw new DomainException($"Ativo {ticker} não encontrado na custódia.");

        posicao.RemoverQuantidade(quantidade);

        if (posicao.Quantidade == 0)
            _posicoes.Remove(posicao);
    }

    public void DistribuirAtivo(string ticker, int quantidade)
    {
        if (Tipo != TipoConta.Master)
            throw new DomainException("Apenas a conta master pode distribuir ativos.");

        var posicao = _posicoes.FirstOrDefault(c => c.Ticker == ticker.ToUpper())
            ?? throw new DomainException($"Ativo {ticker} não encontrado na custódia master.");

        posicao.DistribuirParaFilhote(quantidade);

        if (posicao.Quantidade == 0)
            _posicoes.Remove(posicao);
    }

    public int ObterSaldoResidual(string ticker)
    {
        if (Tipo != TipoConta.Master)
            throw new DomainException("Apenas a conta master possui saldo residual.");

        return _posicoes
            .FirstOrDefault(c => c.Ticker == ticker.ToUpper())
            ?.Quantidade ?? 0;
    }
}
