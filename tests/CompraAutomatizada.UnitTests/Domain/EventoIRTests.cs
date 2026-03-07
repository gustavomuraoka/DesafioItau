using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;
using CompraAutomatizada.Domain.Common;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class EventoIRTests
{
    [Fact]
    public void CriarDedoDuro_Valido_DeveCalcularIrCorretamente()
    {
        var evento = EventoIR.CriarDedoDuro(1, "PETR4", 10000m);

        evento.ValorIr.Should().Be(Math.Round(10000m * 0.00005m, 2));
    }

    [Fact]
    public void CriarDedoDuro_ValorZero_DeveLancarDomainException()
    {
        var act = () => EventoIR.CriarDedoDuro(1, "PETR4", 0m);

        act.Should().Throw<DomainException>().WithMessage("*operação*");
    }

    [Fact]
    public void CriarIrVenda_Valido_DeveCalcularIrSobre20PorCento()
    {
        var evento = EventoIR.CriarIrVenda(1, "PETR4", 30000m, 5000m);

        evento.ValorIr.Should().Be(Math.Round(5000m * 0.20m, 2));
    }

    [Fact]
    public void CriarIrVenda_ValorVendaZero_DeveLancarDomainException()
    {
        var act = () => EventoIR.CriarIrVenda(1, "PETR4", 0m, 1000m);

        act.Should().Throw<DomainException>().WithMessage("*venda*");
    }

    [Fact]
    public void CriarIrVenda_LucroLiquidoNegativo_DeveLancarDomainException()
    {
        var act = () => EventoIR.CriarIrVenda(1, "PETR4", 30000m, -1m);

        act.Should().Throw<DomainException>().WithMessage("*negativo*");
    }

    [Fact]
    public void MarcarComoPublicado_DeveMarcarComoPublicado()
    {
        var evento = EventoIR.CriarDedoDuro(1, "PETR4", 10000m);

        evento.MarcarComoPublicado();

        evento.PublicadoKafka.Should().BeTrue();
    }

    [Fact]
    public void MarcarComoPublicado_JaPublicado_DeveLancarDomainException()
    {
        var evento = EventoIR.CriarDedoDuro(1, "PETR4", 10000m);
        evento.MarcarComoPublicado();

        var act = () => evento.MarcarComoPublicado();

        act.Should().Throw<DomainException>().WithMessage("*publicado*");
    }
}
