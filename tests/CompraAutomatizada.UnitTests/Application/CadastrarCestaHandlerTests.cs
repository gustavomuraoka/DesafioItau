using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Application.UseCases.Cesta.CadastrarCesta;
using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace CompraAutomatizada.UnitTests.Application;

public class CadastrarCestaHandlerTests
{
    private readonly Mock<ICestaTopFiveRepository> _cestaRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IRebalanceamentoService> _rebalanceamentoMock = new();
    private readonly CadastrarCestaHandler _handler;

    private static readonly List<ItemCestaInput> ItensValidos =
    [
        new("PETR4", 20m), new("VALE3", 20m), new("ITUB4", 20m),
        new("BBDC4", 20m), new("WEGE3", 20m)
    ];

    private static IEnumerable<(string, decimal)> ItensTupla(IEnumerable<ItemCestaInput> itens)
        => itens.Select(i => (i.Ticker, i.Percentual));

    public CadastrarCestaHandlerTests()
    {
        _handler = new CadastrarCestaHandler(
            _cestaRepoMock.Object,
            _clienteRepoMock.Object,
            _rebalanceamentoMock.Object);
    }

    [Fact]
    public async Task Handle_PrimeiraCesta_DeveCriarSemRebalanceamento()
    {
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync((CestaTopFive?)null);

        var command = new CadastrarCestaCommand("Top 5", ItensValidos);
        var result = await _handler.Handle(command, default);

        result.RebalanceamentoDisparado.Should().BeFalse();
        result.CestaAnteriorDesativada.Should().BeNull();
        result.Mensagem.Should().Contain("Primeira");
        _rebalanceamentoMock.Verify(
            r => r.RebalancearPorMudancaDeCestaAsync(It.IsAny<CestaTopFive>(), It.IsAny<CestaTopFive>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PrimeiraCesta_DeveSalvarNoCesta()
    {
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync((CestaTopFive?)null);

        var command = new CadastrarCestaCommand("Top 5", ItensValidos);
        await _handler.Handle(command, default);

        _cestaRepoMock.Verify(r => r.AddAsync(It.IsAny<CestaTopFive>(), default), Times.Once);
        _cestaRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_CestaAtiva_DeveDesativarAnteriorEDispararRebalanceamento()
    {
        var cestaAtiva = CestaTopFive.Criar("Antiga", ItensTupla(ItensValidos));
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync(cestaAtiva);
        _clienteRepoMock.Setup(r => r.CountAtivosAsync(default))
            .ReturnsAsync(10);

        var novosItens = new List<ItemCestaInput>
        {
            new("PETR4", 20m), new("VALE3", 20m), new("ITUB4", 20m),
            new("BBDC4", 20m), new("RENT3", 20m)
        };
        var command = new CadastrarCestaCommand("Nova", novosItens);
        var result = await _handler.Handle(command, default);

        result.RebalanceamentoDisparado.Should().BeTrue();
        result.CestaAnteriorDesativada.Should().NotBeNull();
        result.Mensagem.Should().Contain("10");
        _cestaRepoMock.Verify(r => r.UpdateAsync(cestaAtiva, default), Times.Once);
        _rebalanceamentoMock.Verify(
            r => r.RebalancearPorMudancaDeCestaAsync(cestaAtiva, It.IsAny<CestaTopFive>(), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CestaAtiva_DeveIdentificarAtivosRemovidosEAdicionados()
    {
        var cestaAtiva = CestaTopFive.Criar("Antiga", ItensTupla(ItensValidos));
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync(cestaAtiva);
        _clienteRepoMock.Setup(r => r.CountAtivosAsync(default))
            .ReturnsAsync(5);

        // troca WEGE3 por RENT3
        var novosItens = new List<ItemCestaInput>
        {
            new("PETR4", 20m), new("VALE3", 20m), new("ITUB4", 20m),
            new("BBDC4", 20m), new("RENT3", 20m)
        };
        var command = new CadastrarCestaCommand("Nova", novosItens);
        var result = await _handler.Handle(command, default);

        result.AtivosRemovidos.Should().Contain("WEGE3");
        result.AtivosAdicionados.Should().Contain("RENT3");
    }

    [Fact]
    public async Task Handle_MesmosAtivos_DeveRetornarListasVazias()
    {
        var cestaAtiva = CestaTopFive.Criar("Antiga", ItensTupla(ItensValidos));
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync(cestaAtiva);
        _clienteRepoMock.Setup(r => r.CountAtivosAsync(default))
            .ReturnsAsync(3);

        var command = new CadastrarCestaCommand("Nova", ItensValidos);
        var result = await _handler.Handle(command, default);

        result.AtivosRemovidos.Should().BeEmpty();
        result.AtivosAdicionados.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ItensInvalidos_DeveLancarDomainException()
    {
        _cestaRepoMock.Setup(r => r.GetAtivaAsync(default))
            .ReturnsAsync((CestaTopFive?)null);

        var itensInvalidos = new List<ItemCestaInput>
        {
            new("PETR4", 50m), new("VALE3", 50m)
        };
        var command = new CadastrarCestaCommand("Inválida", itensInvalidos);

        var act = () => _handler.Handle(command, default);

        await act.Should().ThrowAsync<DomainException>();
    }
}
