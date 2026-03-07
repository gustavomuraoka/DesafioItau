using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class ContaGraficaTests
{
    [Fact]
    public void CriarFilhote_Valido_DeveCriarComSucesso()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");

        conta.Tipo.Should().Be(TipoConta.Filhote);
        conta.Ativo.Should().BeTrue();
    }

    [Fact]
    public void CriarFilhote_ClienteIdInvalido_DeveLancarDomainException()
    {
        var act = () => ContaGrafica.CriarFilhote(0, "CLI-000001");

        act.Should().Throw<DomainException>().WithMessage("*ClienteId*");
    }

    [Fact]
    public void CriarFilhote_NumeroContaVazio_DeveLancarDomainException()
    {
        var act = () => ContaGrafica.CriarFilhote(1, "");

        act.Should().Throw<DomainException>().WithMessage("*conta*");
    }

    [Fact]
    public void CriarMaster_DeveTerIdFixo()
    {
        var master = ContaGrafica.CriarMaster();

        master.Id.Should().Be(ContaGrafica.IdContaMaster);
        master.Tipo.Should().Be(TipoConta.Master);
    }

    [Fact]
    public void Desativar_ContaAtiva_DeveDesativar()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");

        conta.Desativar();

        conta.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Desativar_ContaJaInativa_DeveLancarDomainException()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        conta.Desativar();

        var act = () => conta.Desativar();

        act.Should().Throw<DomainException>().WithMessage("*inativa*");
    }

    [Fact]
    public void AdicionarOuAtualizarPosicao_NovaPosicao_DeveAdicionarNaLista()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");

        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        conta.Posicoes.Should().HaveCount(1);
        conta.Posicoes.First().Ticker.Should().Be("PETR4");
    }

    [Fact]
    public void AdicionarOuAtualizarPosicao_PosicaoExistente_DeveAtualizarPrecoMedio()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 50m);

        conta.Posicoes.Should().HaveCount(1);
        conta.Posicoes.First().PrecoMedio.Should().Be(40m);
    }

    [Fact]
    public void RemoverPosicao_TickerInexistente_DeveLancarDomainException()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");

        var act = () => conta.RemoverPosicao("PETR4", 10);

        act.Should().Throw<DomainException>().WithMessage("*PETR4*");
    }

    [Fact]
    public void RemoverPosicao_QuantidadeTotal_DeveRemoverDaLista()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        conta.RemoverPosicao("PETR4", 100);

        conta.Posicoes.Should().BeEmpty();
    }

    [Fact]
    public void DistribuirAtivo_ContaNaoMaster_DeveLancarDomainException()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        var act = () => conta.DistribuirAtivo("PETR4", 10);

        act.Should().Throw<DomainException>().WithMessage("*master*");
    }

    [Fact]
    public void DistribuirAtivo_Master_DeveReduzirPosicao()
    {
        var master = ContaGrafica.CriarMaster();
        master.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        master.DistribuirAtivo("PETR4", 40);

        master.Posicoes.First().Quantidade.Should().Be(60);
    }

    [Fact]
    public void ObterSaldoResidual_ContaNaoMaster_DeveLancarDomainException()
    {
        var conta = ContaGrafica.CriarFilhote(1, "CLI-000001");

        var act = () => conta.ObterSaldoResidual("PETR4");

        act.Should().Throw<DomainException>().WithMessage("*master*");
    }

    [Fact]
    public void ObterSaldoResidual_TickerInexistente_DeveRetornarZero()
    {
        var master = ContaGrafica.CriarMaster();

        var saldo = master.ObterSaldoResidual("PETR4");

        saldo.Should().Be(0);
    }
}
