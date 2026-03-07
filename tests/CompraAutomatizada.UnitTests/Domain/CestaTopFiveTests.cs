using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Common;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class CestaTopFiveTests
{
    private static List<(string Ticker, decimal Percentual)> ItensValidos() =>
    [
        ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("WEGE3", 20m)
    ];

    [Fact]
    public void Criar_CestaValida_DeveCriarComSucesso()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());

        cesta.Itens.Should().HaveCount(5);
        cesta.Ativa.Should().BeTrue();
    }

    [Fact]
    public void Criar_MenosDe5Ativos_DeveLancarDomainException()
    {
        var itens = ItensValidos().Take(4).ToList();

        var act = () => CestaTopFive.Criar("Top 5", itens);

        act.Should().Throw<DomainException>().WithMessage("*5 ativos*");
    }

    [Fact]
    public void Criar_TickersDuplicados_DeveLancarDomainException()
    {
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 20m), ("PETR4", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("WEGE3", 20m)
        };

        var act = () => CestaTopFive.Criar("Top 5", itens);

        act.Should().Throw<DomainException>().WithMessage("*duplicados*");
    }

    [Fact]
    public void Criar_SomaPercentuaisDiferenteDe100_DeveLancarDomainException()
    {
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 10m), ("VALE3", 10m), ("ITUB4", 10m), ("BBDC4", 10m), ("WEGE3", 10m)
        };

        var act = () => CestaTopFive.Criar("Top 5", itens);

        act.Should().Throw<DomainException>().WithMessage("*100%*");
    }

    [Fact]
    public void Criar_NomeVazio_DeveLancarDomainException()
    {
        var act = () => CestaTopFive.Criar("", ItensValidos());

        act.Should().Throw<DomainException>().WithMessage("*Nome*");
    }

    [Fact]
    public void Desativar_CestaAtiva_DeveDesativar()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());

        cesta.Desativar();

        cesta.Ativa.Should().BeFalse();
        cesta.DataDesativacao.Should().NotBeNull();
    }

    [Fact]
    public void Desativar_CestaJaInativa_DeveLancarDomainException()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos());
        cesta.Desativar();

        var act = () => cesta.Desativar();

        act.Should().Throw<DomainException>().WithMessage("*inativa*");
    }
}
