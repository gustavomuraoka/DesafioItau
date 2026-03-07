using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Common;
using FluentAssertions;

namespace CompraAutomatizada.UnitTests.Domain;

public class DistribuicaoTests
{
    [Fact]
    public void Criar_DeveCalcularIrDedoDuro_Corretamente()
    {
        var dist = Distribuicao.Criar(1, 1, 1, "PETR4", 100, 30m, DateTime.UtcNow);

        dist.ValorIrDedoDuro.Should().Be(Math.Round(3000m * 0.00005m, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComQuantidadeInvalida_DeveLancarDomainException(int quantidade)
    {
        var act = () => Distribuicao.Criar(1, 1, 1, "PETR4", quantidade, 30m, DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComTickerVazio_DeveLancarDomainException()
    {
        var act = () => Distribuicao.Criar(1, 1, 1, "", 10, 30m, DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComPrecoUnitarioZero_DeveLancarDomainException()
    {
        var act = () => Distribuicao.Criar(1, 1, 1, "PETR4", 10, 0m, DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ValorTotal_DeveSerQuantidadeVezesPreco()
    {
        var dist = Distribuicao.Criar(1, 1, 1, "PETR4", 50, 20m, DateTime.UtcNow);

        dist.ValorTotal.Should().Be(1000m);
    }

    [Fact]
    public void Criar_DeveNormalizarTickerParaMaiusculo()
    {
        var dist = Distribuicao.Criar(1, 1, 1, "petr4", 10, 20m, DateTime.UtcNow);

        dist.Ticker.Should().Be("PETR4");
    }
}
