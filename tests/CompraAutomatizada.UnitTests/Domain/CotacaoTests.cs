using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Common;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class CotacaoTests
{
    private static readonly DateOnly DataPregao = new(2026, 1, 5);

    [Fact]
    public void Criar_Valido_DeveCriarComSucesso()
    {
        var cotacao = Cotacao.Criar("PETR4", DataPregao, 29m, 30m, 31m, 28m, 1000);

        cotacao.Ticker.Should().Be("PETR4");
        cotacao.PrecoFechamento.Should().Be(30m);
    }

    [Fact]
    public void Criar_TickerVazio_DeveLancarDomainException()
    {
        var act = () => Cotacao.Criar("", DataPregao, 29m, 30m, 31m, 28m, 1000);

        act.Should().Throw<DomainException>().WithMessage("*Ticker*");
    }

    [Fact]
    public void Criar_PrecoFechamentoZero_DeveLancarDomainException()
    {
        var act = () => Cotacao.Criar("PETR4", DataPregao, 29m, 0m, 31m, 28m, 1000);

        act.Should().Throw<DomainException>().WithMessage("*fechamento*");
    }

    [Fact]
    public void Criar_PrecoMaximoMenorQueMinimo_DeveLancarDomainException()
    {
        var act = () => Cotacao.Criar("PETR4", DataPregao, 29m, 30m, 27m, 28m, 1000);

        act.Should().Throw<DomainException>().WithMessage("*máximo*");
    }

    [Fact]
    public void Criar_DeveNormalizarTickerParaMaiusculo()
    {
        var cotacao = Cotacao.Criar("petr4", DataPregao, 29m, 30m, 31m, 28m, 1000);

        cotacao.Ticker.Should().Be("PETR4");
    }
}
