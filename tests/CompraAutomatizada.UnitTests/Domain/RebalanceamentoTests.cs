using CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class RebalanceamentoTests
{
    [Fact]
    public void Criar_Valido_DeveCriarComSucesso()
    {
        var r = Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "PETR4", 10, 0, 30m, 5000m, 25m);

        r.Ticker.Should().Be("PETR4");
        r.QuantidadeVendida.Should().Be(10);
    }

    [Fact]
    public void Criar_ClienteIdInvalido_DeveLancarDomainException()
    {
        var act = () => Rebalanceamento.Criar(0, TipoRebalanceamento.Desvio, "PETR4", 10, 0, 30m, 5000m, 25m);

        act.Should().Throw<DomainException>().WithMessage("*ClienteId*");
    }

    [Fact]
    public void Criar_TickerVazio_DeveLancarDomainException()
    {
        var act = () => Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "", 10, 0, 30m, 5000m, 25m);

        act.Should().Throw<DomainException>().WithMessage("*Ticker*");
    }

    [Fact]
    public void Criar_SemNenhumaOperacao_DeveLancarDomainException()
    {
        var act = () => Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "PETR4", 0, 0, 30m, 5000m, 25m);

        act.Should().Throw<DomainException>().WithMessage("*operação*");
    }

    [Fact]
    public void Criar_PrecoZero_DeveLancarDomainException()
    {
        var act = () => Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "PETR4", 10, 0, 0m, 5000m, 25m);

        act.Should().Throw<DomainException>().WithMessage("*Preço*");
    }

    [Fact]
    public void Criar_VendaAcimaLimite_ComLucro_DeveCalcularIR()
    {
        var r = Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "PETR4", 100, 0, 40m, 25000m, 25m);

        r.ValorIrApurado.Should().Be(Math.Round(1500m * 0.20m, 2));
    }

    [Fact]
    public void Criar_VendaAbaixoLimite_NaoDeveCalcularIR()
    {
        var r = Rebalanceamento.Criar(1, TipoRebalanceamento.Desvio, "PETR4", 10, 0, 30m, 15000m, 25m);

        r.ValorIrApurado.Should().BeNull();
    }
}
