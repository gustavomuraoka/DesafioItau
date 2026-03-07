using CompraAutomatizada.Application.UseCases.Admin.ConsultarCustodiaMaster;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace CompraAutomatizada.UnitTests.Application;

public class ConsultarCustodiaMasterHandlerTests
{
    private readonly Mock<IContaGraficaRepository> _contaRepoMock = new();
    private readonly Mock<ICotacaoRepository> _cotacaoRepoMock = new();
    private readonly ConsultarCustodiaMasterHandler _handler;

    public ConsultarCustodiaMasterHandlerTests()
    {
        _handler = new ConsultarCustodiaMasterHandler(
            _contaRepoMock.Object,
            _cotacaoRepoMock.Object);
    }

    [Fact]
    public async Task Handle_SemContaMaster_DeveLancarDomainException()
    {
        _contaRepoMock.Setup(r => r.GetMasterAsync(default))
            .ReturnsAsync((ContaGrafica?)null);

        var act = () => _handler.Handle(new ConsultarCustodiaMasterQuery(), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*master*");
    }

    [Fact]
    public async Task Handle_MasterSemPosicoes_DeveRetornarDtoComValorZero()
    {
        var master = ContaGrafica.CriarMaster();
        _contaRepoMock.Setup(r => r.GetMasterAsync(default))
            .ReturnsAsync(master);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCustodiaMasterQuery(), default);

        result.Should().NotBeNull();
        result.Custodia.Should().BeEmpty();
        result.ValorTotalResiduo.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_MasterComPosicoes_DeveCalcularValorComCotacao()
    {
        var master = ContaGrafica.CriarMaster();
        master.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        _contaRepoMock.Setup(r => r.GetMasterAsync(default))
            .ReturnsAsync(master);

        var cotacao = Cotacao.Criar("PETR4", new DateOnly(2026, 1, 10), 39m, 40m, 41m, 38m, 1000);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([cotacao]);

        var result = await _handler.Handle(new ConsultarCustodiaMasterQuery(), default);

        var posicao = result.Custodia.First();
        posicao.Ticker.Should().Be("PETR4");
        posicao.ValorAtual.Should().Be(4000m); // 100 * 40
        result.ValorTotalResiduo.Should().Be(4000m);
    }

    [Fact]
    public async Task Handle_TickerSemCotacao_DeveUsarPrecoMedioComoFallback()
    {
        var master = ContaGrafica.CriarMaster();
        master.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        _contaRepoMock.Setup(r => r.GetMasterAsync(default))
            .ReturnsAsync(master);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCustodiaMasterQuery(), default);

        result.Custodia.First().ValorAtual.Should().Be(3000m); // 100 * 30 (preço médio)
    }

    [Fact]
    public async Task Handle_MultiplasPosicoesComCotacoes_DeveSomarValorTotal()
    {
        var master = ContaGrafica.CriarMaster();
        master.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);
        master.AdicionarOuAtualizarPosicao("VALE3", 50, 80m);

        _contaRepoMock.Setup(r => r.GetMasterAsync(default))
            .ReturnsAsync(master);

        var cotacoes = new List<Cotacao>
        {
            Cotacao.Criar("PETR4", new DateOnly(2026, 1, 10), 39m, 40m, 41m, 38m, 1000),
            Cotacao.Criar("VALE3", new DateOnly(2026, 1, 10), 89m, 90m, 91m, 88m, 500)
        };
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync(cotacoes);

        var result = await _handler.Handle(new ConsultarCustodiaMasterQuery(), default);

        result.Custodia.Should().HaveCount(2);
        result.ValorTotalResiduo.Should().Be(8500m); // 100*40 + 50*90
    }
}
