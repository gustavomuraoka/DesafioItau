using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Application.UseCases.Motor.ExecutarCompra;
using FluentAssertions;
using Moq;

namespace CompraAutomatizada.UnitTests.Application;

public class ExecutarCompraHandlerTests
{
    private readonly Mock<ICompraService> _compraServiceMock = new();
    private readonly ExecutarCompraHandler _handler;

    public ExecutarCompraHandlerTests()
    {
        _handler = new ExecutarCompraHandler(_compraServiceMock.Object);
    }

    private static ExecucaoCompraDto CriarDto(int totalClientes = 5, string mensagem = "Sucesso")
        => new(
            DateTime.UtcNow,
            totalClientes,
            50000m,
            [],
            [],
            [],
            10,
            mensagem
        );

    [Fact]
    public async Task Handle_DeveDelegarAoCompraService()
    {
        var data = new DateOnly(2026, 1, 10);
        _compraServiceMock.Setup(s => s.ExecutarAsync(data, default))
            .ReturnsAsync(CriarDto());

        await _handler.Handle(new ExecutarCompraCommand(data), default);

        _compraServiceMock.Verify(s => s.ExecutarAsync(data, default), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarResultadoDoService()
    {
        var data = new DateOnly(2026, 1, 10);
        var dto = CriarDto(totalClientes: 10, mensagem: "Compra executada com sucesso.");
        _compraServiceMock.Setup(s => s.ExecutarAsync(data, default))
            .ReturnsAsync(dto);

        var result = await _handler.Handle(new ExecutarCompraCommand(data), default);

        result.Should().Be(dto);
        result.TotalClientes.Should().Be(10);
        result.Mensagem.Should().Be("Compra executada com sucesso.");
    }

    [Fact]
    public async Task Handle_DataDiferente_DevePassarDataCorretaAoService()
    {
        var data = new DateOnly(2026, 3, 7);
        _compraServiceMock.Setup(s => s.ExecutarAsync(data, default))
            .ReturnsAsync(CriarDto());

        await _handler.Handle(new ExecutarCompraCommand(data), default);

        _compraServiceMock.Verify(s => s.ExecutarAsync(
            It.Is<DateOnly>(d => d == data), default), Times.Once);
    }
}
