using CompraAutomatizada.Application.UseCases.Cesta.ConsultarCestaAtiva;
using CompraAutomatizada.Application.UseCases.Cesta.ConsultarHistoricoCestas;
using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace CompraAutomatizada.UnitTests.Application;

public class ConsultarCestaAtivaHandlerTests
{
    private readonly Mock<ICestaTopFiveRepository> _cestaRepoMock = new();
    private readonly Mock<ICotacaoRepository> _cotacaoRepoMock = new();
    private readonly ConsultarCestaAtivaHandler _handler;

    private static IEnumerable<(string, decimal)> ItensValidos =>
    [
        ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("WEGE3", 20m)
    ];

    public ConsultarCestaAtivaHandlerTests()
    {
        _handler = new ConsultarCestaAtivaHandler(
            _cestaRepoMock.Object,
            _cotacaoRepoMock.Object);
    }

    [Fact]
    public async Task Handle_SemCestaAtiva_DeveLancarDomainException()
    {
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync((CestaTopFive?)null);

        var act = () => _handler.Handle(new ConsultarCestaAtivaQuery(), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*ativa*");
    }

    [Fact]
    public async Task Handle_CestaAtiva_DeveRetornarDtoCorreto()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos);
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default)).ReturnsAsync(cesta);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCestaAtivaQuery(), default);

        result.Nome.Should().Be("Top 5");
        result.Ativa.Should().BeTrue();
        result.Itens.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ComCotacoes_DevePreencherCotacaoAtual()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos);
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default)).ReturnsAsync(cesta);

        var cotacoes = new List<Cotacao>
        {
            Cotacao.Criar("PETR4", new DateOnly(2026, 1, 10), 29m, 30m, 31m, 28m, 1000)
        };
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync(cotacoes);

        var result = await _handler.Handle(new ConsultarCestaAtivaQuery(), default);

        result.Itens.First(i => i.Ticker == "PETR4").CotacaoAtual.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_TickerSemCotacao_DeveRetornarCotacaoZero()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos);
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default)).ReturnsAsync(cesta);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCestaAtivaQuery(), default);

        result.Itens.Should().OnlyContain(i => i.CotacaoAtual == 0m);
    }

    [Fact]
    public async Task Handle_DevePreencherPercentuaisCorretamente()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos);
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default)).ReturnsAsync(cesta);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCestaAtivaQuery(), default);

        result.Itens.Should().OnlyContain(i => i.Percentual == 20m);
    }
}

public class ConsultarHistoricoCestasHandlerTests
{
    private readonly Mock<ICestaTopFiveRepository> _cestaRepoMock = new();
    private readonly ConsultarHistoricoCestasHandler _handler;

    private static IEnumerable<(string, decimal)> ItensValidos =>
    [
        ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("WEGE3", 20m)
    ];

    public ConsultarHistoricoCestasHandlerTests()
    {
        _handler = new ConsultarHistoricoCestasHandler(_cestaRepoMock.Object);
    }

    [Fact]
    public async Task Handle_SemCestas_DeveRetornarListaVazia()
    {
        _cestaRepoMock.Setup(r => r.GetHistoricoAsync(default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarHistoricoCestasQuery(), default);

        result.Cestas.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ComCestas_DeveRetornarTodasOrdenadas()
    {
        var cesta1 = CestaTopFive.Criar("Cesta 1", ItensValidos);
        await Task.Delay(10);
        var cesta2 = CestaTopFive.Criar("Cesta 2", ItensValidos);

        _cestaRepoMock.Setup(r => r.GetHistoricoAsync(default))
            .ReturnsAsync([cesta1, cesta2]);

        var result = await _handler.Handle(new ConsultarHistoricoCestasQuery(), default);

        var lista = result.Cestas.ToList();
        lista.Should().HaveCount(2);
        lista[0].DataCriacao.Should().BeOnOrAfter(lista[1].DataCriacao);
    }

    [Fact]
    public async Task Handle_DeveMapearItensCorretamente()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos);
        _cestaRepoMock.Setup(r => r.GetHistoricoAsync(default))
            .ReturnsAsync([cesta]);

        var result = await _handler.Handle(new ConsultarHistoricoCestasQuery(), default);

        result.Cestas.First().Itens.Should().HaveCount(5);
        result.Cestas.First().Itens.Should().Contain(i => i.Ticker == "PETR4" && i.Percentual == 20m);
    }

    [Fact]
    public async Task Handle_CestaDesativada_DevePreencherDataDesativacao()
    {
        var cesta = CestaTopFive.Criar("Top 5", ItensValidos);
        cesta.Desativar();
        _cestaRepoMock.Setup(r => r.GetHistoricoAsync(default))
            .ReturnsAsync([cesta]);

        var result = await _handler.Handle(new ConsultarHistoricoCestasQuery(), default);

        result.Cestas.First().DataDesativacao.Should().NotBeNull();
        result.Cestas.First().Ativa.Should().BeFalse();
    }
}
