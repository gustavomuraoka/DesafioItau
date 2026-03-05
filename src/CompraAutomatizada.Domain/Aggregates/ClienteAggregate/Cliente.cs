using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.ValueObjects;

namespace CompraAutomatizada.Domain.Aggregates.ClienteAggregate;

public class Cliente : Entity
{
    public string Nome { get; private set; } = string.Empty;
    public Cpf Cpf { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public decimal ValorMensal { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataAdesao { get; private set; }

    private ContaGrafica? _conta;
    public ContaGrafica? Conta => _conta;

    protected Cliente() { }

    private Cliente(string nome, Cpf cpf, string email, decimal valorMensal)
    {
        ValidarNome(nome);
        ValidarEmail(email);
        ValidarValorMensal(valorMensal);

        Nome = nome;
        Cpf = cpf;
        Email = email;
        ValorMensal = valorMensal;
        Ativo = true;
        DataAdesao = DateTime.UtcNow;
    }

    public static Cliente Criar(string nome, string cpf, string email, decimal valorMensal)
        => new(nome, new Cpf(cpf), email, valorMensal);

    public ContaGrafica AbrirConta(string numeroConta)
    {
        if (!Ativo)
            throw new DomainException("Cliente inativo não pode abrir conta.");

        if (_conta is not null && _conta.Ativo)
            throw new DomainException("Cliente já possui uma conta ativa.");

        _conta = ContaGrafica.CriarFilhote(Id, numeroConta);
        return _conta;
    }

    public void Sair()
    {
        if (!Ativo)
            throw new DomainException("Cliente já está inativo.");

        Ativo = false;
        _conta?.Desativar();
    }

    public void AlterarValorMensal(decimal novoValor)
    {
        if (!Ativo)
            throw new DomainException("Cliente inativo não pode alterar o aporte.");

        ValidarValorMensal(novoValor);
        ValorMensal = novoValor;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");
        if (nome.Length > 200)
            throw new DomainException("Nome não pode ter mais de 200 caracteres.");
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("E-mail é obrigatório.");
        if (!email.Contains('@') || !email.Contains('.'))
            throw new DomainException("E-mail inválido.");
    }

    private static void ValidarValorMensal(decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Valor mensal deve ser maior que zero.");
    }
}
