using CompraAutomatizada.Application.UseCases.Clientes.ConsultarRentabilidade;
using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace CompraAutomatizada.UnitTests.Handlers;

public class ConsultarRentabilidadeHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IContaGraficaRepository> _contaRepo = new();
    private readonly Mock<ICotacaoRepository> _cotacaoRepo = new();
    private readonly Mock<IOrdemDeCompraRepository> _ordemRepo = new();

    private ConsultarRentabilidadeHandler CreateHandler() =>
        new(_clienteRepo.Object, _contaRepo.Object, _cotacaoRepo.Object, _ordemRepo.Object);

    private static Cliente CriarClienteFake()
        => Cliente.Criar("Gustavo", "517.242.858-58", "gustavo@email.com", 1000m);

    private static ContaGrafica CriarContaFake()
        => ContaGrafica.CriarFilhote(1, "CLI-000001");

    [Fact]
    public async Task Handle_ClienteNaoEncontrado_DeveLancarDomainException()
    {
        _clienteRepo
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync((Cliente?)null);

        var act = async () => await CreateHandler().Handle(new ConsultarRentabilidadeQuery(1), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Cliente*");
    }

    [Fact]
    public async Task Handle_ContaNaoEncontrada_DeveLancarDomainException()
    {
        _clienteRepo
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(CriarClienteFake());
        _contaRepo
            .Setup(r => r.GetByClienteIdAsync(1, default))
            .ReturnsAsync((ContaGrafica?)null);

        var act = async () => await CreateHandler().Handle(new ConsultarRentabilidadeQuery(1), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*conta*");
    }

    [Fact]
    public async Task Handle_SemDistribuicoes_RetornaResumoZerado()
    {
        _clienteRepo
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(CriarClienteFake());
        _contaRepo
            .Setup(r => r.GetByClienteIdAsync(1, default))
            .ReturnsAsync(CriarContaFake());
        _cotacaoRepo
            .Setup(r => r.GetUltimasByTickersAsync(It.IsAny<List<string>>(), default))
            .ReturnsAsync(Enumerable.Empty<Cotacao>());
        _ordemRepo
            .Setup(r => r.GetDistribuicoesByClienteIdAsync(1, default))
            .ReturnsAsync(Enumerable.Empty<Distribuicao>());

        var result = await CreateHandler().Handle(new ConsultarRentabilidadeQuery(1), default);

        result.Rentabilidade.ValorTotalInvestido.Should().Be(0);
        result.HistoricoAportes.Should().BeEmpty();
        result.EvolucaoCarteira.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ComDistribuicoes_RetornaHistoricoAgrupado()
    {
        var cliente = CriarClienteFake();
        var conta = CriarContaFake();
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        var data1 = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var data2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        var distribuicoes = new List<Distribuicao>
        {
            Distribuicao.Criar(1, 1, 1, "PETR4", 50, 30m, data1),
            Distribuicao.Criar(2, 1, 1, "PETR4", 50, 30m, data2),
        };

        _clienteRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(cliente);
        _contaRepo.Setup(r => r.GetByClienteIdAsync(1, default)).ReturnsAsync(conta);
        _cotacaoRepo
            .Setup(r => r.GetUltimasByTickersAsync(It.IsAny<List<string>>(), default))
            .ReturnsAsync(Enumerable.Empty<Cotacao>());
        _ordemRepo
            .Setup(r => r.GetDistribuicoesByClienteIdAsync(1, default))
            .ReturnsAsync(distribuicoes);

        var result = await CreateHandler().Handle(new ConsultarRentabilidadeQuery(1), default);

        result.HistoricoAportes.Should().HaveCount(2);
        var aportes = result.HistoricoAportes.ToList();
        aportes[0].Parcela.Should().Be("1/2");
        aportes[1].Parcela.Should().Be("2/2");
    }
}
