using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Enums;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class OrdemDeCompraTests
{
    [Fact]
    public void Criar_OrdemLoteValida_DeveCriarComSucesso()
    {
        var ordem = OrdemDeCompra.Criar(1, "PETR4", 100, 30m, TipoMercado.Lote);

        ordem.Ticker.Should().Be("PETR4");
        ordem.ValorTotal.Should().Be(3000m);
    }

    [Fact]
    public void Criar_LoteNaoMultiploDe100_DeveLancarDomainException()
    {
        var act = () => OrdemDeCompra.Criar(1, "PETR4", 150, 30m, TipoMercado.Lote);

        act.Should().Throw<DomainException>().WithMessage("*100*");
    }

    [Fact]
    public void Criar_FracionarioComQuantidadeMaiorOuIgual100_DeveLancarDomainException()
    {
        var act = () => OrdemDeCompra.Criar(1, "PETR4F", 100, 30m, TipoMercado.Fracionario);

        act.Should().Throw<DomainException>().WithMessage("*fracion*");
    }

    [Fact]
    public void Criar_TickerVazio_DeveLancarDomainException()
    {
        var act = () => OrdemDeCompra.Criar(1, "", 100, 30m, TipoMercado.Lote);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_PrecoZero_DeveLancarDomainException()
    {
        var act = () => OrdemDeCompra.Criar(1, "PETR4", 100, 0m, TipoMercado.Lote);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CriarComSplit_QuantidadePura_DeveCriarSoLote()
    {
        var ordens = OrdemDeCompra.CriarComSplit(1, "PETR4", 200, 30m).ToList();

        ordens.Should().HaveCount(1);
        ordens[0].TipoMercado.Should().Be(TipoMercado.Lote);
    }

    [Fact]
    public void CriarComSplit_QuantidadeMista_DeveCriarLoteEFracionario()
    {
        var ordens = OrdemDeCompra.CriarComSplit(1, "PETR4", 150, 30m).ToList();

        ordens.Should().HaveCount(2);
        ordens.Should().Contain(o => o.TipoMercado == TipoMercado.Lote);
        ordens.Should().Contain(o => o.TipoMercado == TipoMercado.Fracionario);
    }

    [Fact]
    public void CriarComSplit_QuantidadePuraFracionaria_DeveCriarSoFracionario()
    {
        var ordens = OrdemDeCompra.CriarComSplit(1, "PETR4", 50, 30m).ToList();

        ordens.Should().HaveCount(1);
        ordens[0].TipoMercado.Should().Be(TipoMercado.Fracionario);
    }

    [Fact]
    public void AdicionarDistribuicao_DeveAdicionarNaLista()
    {
        var ordem = OrdemDeCompra.Criar(1, "PETR4", 100, 30m, TipoMercado.Lote);
        ordem.Id = 1;

        ordem.AdicionarDistribuicao(1, 1, 50, 30m, DateTime.UtcNow);

        ordem.Distribuicoes.Should().HaveCount(1);
    }
}
