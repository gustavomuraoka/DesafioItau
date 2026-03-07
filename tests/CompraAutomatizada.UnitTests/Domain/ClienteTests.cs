using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.Common;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class ClienteTests
{
    private static Cliente CriarClienteComId(long id = 1)
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);
        cliente.Id = id;
        return cliente;
    }

    [Fact]
    public void Criar_ClienteValido_DeveCriarComSucesso()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);

        cliente.Nome.Should().Be("Gustavo");
        cliente.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_NomeVazio_DeveLancarDomainException()
    {
        var act = () => Cliente.Criar("", "529.982.247-25", "gustavo@email.com", 1000m);

        act.Should().Throw<DomainException>().WithMessage("*Nome*");
    }

    [Fact]
    public void Criar_NomeComMaisDe200Caracteres_DeveLancarDomainException()
    {
        var nomeGrande = new string('A', 201);
        var act = () => Cliente.Criar(nomeGrande, "529.982.247-25", "gustavo@email.com", 1000m);

        act.Should().Throw<DomainException>().WithMessage("*200*");
    }

    [Fact]
    public void Criar_EmailSemArroba_DeveLancarDomainException()
    {
        var act = () => Cliente.Criar("Gustavo", "529.982.247-25", "gustavoemail.com", 1000m);

        act.Should().Throw<DomainException>().WithMessage("*E-mail*");
    }

    [Fact]
    public void Criar_EmailSemPonto_DeveLancarDomainException()
    {
        var act = () => Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@emailcom", 1000m);

        act.Should().Throw<DomainException>().WithMessage("*E-mail*");
    }

    [Fact]
    public void Criar_ValorMensalZero_DeveLancarDomainException()
    {
        var act = () => Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 0m);

        act.Should().Throw<DomainException>().WithMessage("*Valor mensal*");
    }

    [Fact]
    public void AbrirConta_ClienteAtivo_DeveCriarConta()
    {
        var cliente = CriarClienteComId();

        var conta = cliente.AbrirConta("CLI-000001");

        conta.Should().NotBeNull();
        cliente.Conta.Should().NotBeNull();
    }

    [Fact]
    public void AbrirConta_ClienteInativo_DeveLancarDomainException()
    {
        var cliente = CriarClienteComId();
        cliente.Sair();

        var act = () => cliente.AbrirConta("CLI-000001");

        act.Should().Throw<DomainException>().WithMessage("*inativo*");
    }

    [Fact]
    public void AbrirConta_JaPossuiContaAtiva_DeveLancarDomainException()
    {
        var cliente = CriarClienteComId();
        cliente.AbrirConta("CLI-000001");

        var act = () => cliente.AbrirConta("CLI-000002");

        act.Should().Throw<DomainException>().WithMessage("*conta ativa*");
    }

    [Fact]
    public void Sair_ClienteAtivo_DeveDesativar()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);

        cliente.Sair();

        cliente.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Sair_ClienteJaInativo_DeveLancarDomainException()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);
        cliente.Sair();

        var act = () => cliente.Sair();

        act.Should().Throw<DomainException>().WithMessage("*inativo*");
    }

    [Fact]
    public void AlterarValorMensal_ClienteAtivo_DeveAtualizar()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);

        cliente.AlterarValorMensal(2000m);

        cliente.ValorMensal.Should().Be(2000m);
    }

    [Fact]
    public void AlterarValorMensal_ClienteInativo_DeveLancarDomainException()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "gustavo@email.com", 1000m);
        cliente.Sair();

        var act = () => cliente.AlterarValorMensal(2000m);

        act.Should().Throw<DomainException>().WithMessage("*inativo*");
    }
}
