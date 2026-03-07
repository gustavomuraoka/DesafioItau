using CompraAutomatizada.Application.UseCases.Clientes.AderirAoProduto;
using CompraAutomatizada.Application.UseCases.Clientes.AlterarValorMensal;
using CompraAutomatizada.Application.UseCases.Clientes.ConsultarCarteira;
using CompraAutomatizada.Application.UseCases.Clientes.SairDoProduto;
using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace CompraAutomatizada.UnitTests.Application;

public class AderirAoProdutoHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IContaGraficaRepository> _contaRepoMock = new();
    private readonly AderirAoProdutoHandler _handler;

    public AderirAoProdutoHandlerTests()
    {
        _handler = new AderirAoProdutoHandler(_clienteRepoMock.Object, _contaRepoMock.Object);
    }

    private static void SetId(object entity, long id)
        => entity.GetType().BaseType!.GetProperty("Id")!.SetValue(entity, id);

    [Fact]
    public async Task Handle_CpfJaCadastrado_DeveLancarDomainException()
    {
        var clienteExistente = Cliente.Criar("Outro", "529.982.247-25", "outro@email.com", 500m);
        _clienteRepoMock.Setup(r => r.GetByCpfAsync(It.IsAny<string>(), default))
            .ReturnsAsync(clienteExistente);

        var command = new AderirAoProdutoCommand("Gustavo", "529.982.247-25", "g@email.com", 1000m);

        var act = () => _handler.Handle(command, default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*cadastrado*");
    }

    [Fact]
    public async Task Handle_ClienteNovo_DevePersistirESalvarConta()
    {
        _clienteRepoMock.Setup(r => r.GetByCpfAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Cliente?)null);
        _clienteRepoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>(), default))
            .Callback<Cliente, CancellationToken>((c, _) => SetId(c, 1L));

        var command = new AderirAoProdutoCommand("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        await _handler.Handle(command, default);

        _clienteRepoMock.Verify(r => r.AddAsync(It.IsAny<Cliente>(), default), Times.Once);
        _contaRepoMock.Verify(r => r.AddAsync(It.IsAny<ContaGrafica>(), default), Times.Once);
        _clienteRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ClienteNovo_DeveRetornarResponseCorreto()
    {
        _clienteRepoMock.Setup(r => r.GetByCpfAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Cliente?)null);
        _clienteRepoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>(), default))
            .Callback<Cliente, CancellationToken>((c, _) => SetId(c, 1L));

        var command = new AderirAoProdutoCommand("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        var result = await _handler.Handle(command, default);

        result.Nome.Should().Be("Gustavo");
        result.Cpf.Should().Be("52998224725");
        result.Email.Should().Be("g@email.com");
        result.ValorMensal.Should().Be(1000m);
        result.Ativo.Should().BeTrue();
        result.ContaGrafica.Should().NotBeNull();
        result.ContaGrafica.NumeroConta.Should().StartWith("FLH-");
    }
}

public class AlterarValorMensalHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly AlterarValorMensalHandler _handler;

    public AlterarValorMensalHandlerTests()
    {
        _handler = new AlterarValorMensalHandler(_clienteRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ClienteInexistente_DeveLancarDomainException()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync((Cliente?)null);

        var act = () => _handler.Handle(new AlterarValorMensalCommand(1, 2000m), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task Handle_ClienteExistente_DeveAtualizarValor()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default))
            .ReturnsAsync(cliente);

        var result = await _handler.Handle(new AlterarValorMensalCommand(1, 2000m), default);

        result.ValorMensalAnterior.Should().Be(1000m);
        result.ValorMensalNovo.Should().Be(2000m);
        _clienteRepoMock.Verify(r => r.UpdateAsync(cliente, default), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteExistente_DeveRetornarMensagem()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default))
            .ReturnsAsync(cliente);

        var result = await _handler.Handle(new AlterarValorMensalCommand(1, 2000m), default);

        result.Mensagem.Should().NotBeNullOrWhiteSpace();
    }
}

public class ConsultarCarteiraHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IContaGraficaRepository> _contaRepoMock = new();
    private readonly Mock<ICotacaoRepository> _cotacaoRepoMock = new();
    private readonly ConsultarCarteiraHandler _handler;

    public ConsultarCarteiraHandlerTests()
    {
        _handler = new ConsultarCarteiraHandler(
            _clienteRepoMock.Object,
            _contaRepoMock.Object,
            _cotacaoRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ClienteInexistente_DeveLancarDomainException()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync((Cliente?)null);

        var act = () => _handler.Handle(new ConsultarCarteiraQuery(1), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Cliente*");
    }

    [Fact]
    public async Task Handle_ContaInexistente_DeveLancarDomainException()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default))
            .ReturnsAsync(cliente);
        _contaRepoMock.Setup(r => r.GetByClienteIdAsync(It.IsAny<long>(), default))
            .ReturnsAsync((ContaGrafica?)null);

        var act = () => _handler.Handle(new ConsultarCarteiraQuery(1), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Conta*");
    }

    [Fact]
    public async Task Handle_CarteiraVazia_DeveRetornarResumoZerado()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        var conta = ContaGrafica.CriarFilhote(1, "FLH-000001");
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(cliente);
        _contaRepoMock.Setup(r => r.GetByClienteIdAsync(It.IsAny<long>(), default)).ReturnsAsync(conta);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCarteiraQuery(1), default);

        result.Ativos.Should().BeEmpty();
        result.Resumo.ValorTotalInvestido.Should().Be(0m);
        result.Resumo.ValorAtualCarteira.Should().Be(0m);
        result.Resumo.PlTotal.Should().Be(0m);
        result.Resumo.RentabilidadePercentual.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_ComPosicoes_DeveCalcularPLCorretamente()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        var conta = ContaGrafica.CriarFilhote(1, "FLH-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(cliente);
        _contaRepoMock.Setup(r => r.GetByClienteIdAsync(It.IsAny<long>(), default)).ReturnsAsync(conta);

        var cotacao = Cotacao.Criar("PETR4", new DateOnly(2026, 1, 10), 39m, 40m, 41m, 38m, 1000);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([cotacao]);

        var result = await _handler.Handle(new ConsultarCarteiraQuery(1), default);

        var ativo = result.Ativos.First();
        ativo.Ticker.Should().Be("PETR4");
        ativo.ValorAtual.Should().Be(4000m);
        ativo.Pl.Should().Be(1000m);
        ativo.PlPercentual.Should().Be(33.33m);
        result.Resumo.ValorAtualCarteira.Should().Be(4000m);
        result.Resumo.PlTotal.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_TickerSemCotacao_DeveUsarPrecoMedioComoFallback()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        var conta = ContaGrafica.CriarFilhote(1, "FLH-000001");
        conta.AdicionarOuAtualizarPosicao("PETR4", 100, 30m);

        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(cliente);
        _contaRepoMock.Setup(r => r.GetByClienteIdAsync(It.IsAny<long>(), default)).ReturnsAsync(conta);
        _cotacaoRepoMock.Setup(r => r.GetUltimasByTickersAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ConsultarCarteiraQuery(1), default);

        result.Ativos.First().CotacaoAtual.Should().Be(30m);
        result.Resumo.PlTotal.Should().Be(0m);
    }
}

public class SairDoProdutoHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IContaGraficaRepository> _contaRepoMock = new();
    private readonly SairDoProdutoHandler _handler;

    public SairDoProdutoHandlerTests()
    {
        _handler = new SairDoProdutoHandler(_clienteRepoMock.Object, _contaRepoMock.Object);
    }

    private static void SetId(object entity, long id)
        => entity.GetType().BaseType!.GetProperty("Id")!.SetValue(entity, id);

    [Fact]
    public async Task Handle_ClienteInexistente_DeveLancarDomainException()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync((Cliente?)null);

        var act = () => _handler.Handle(new SairDoProdutoCommand(1), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task Handle_ClienteAtivo_DeveDesativarERetornarResponse()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(cliente);

        var result = await _handler.Handle(new SairDoProdutoCommand(1), default);

        result.Ativo.Should().BeFalse();
        result.Nome.Should().Be("Gustavo");
        result.Mensagem.Should().NotBeNullOrWhiteSpace();
        _clienteRepoMock.Verify(r => r.UpdateAsync(cliente, default), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteComConta_DeveAtualizarConta()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        SetId(cliente, 1L);
        cliente.AbrirConta("FLH-000001");
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(cliente);

        await _handler.Handle(new SairDoProdutoCommand(1), default);

        _contaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ContaGrafica>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteSemConta_NaoDeveAtualizarConta()
    {
        var cliente = Cliente.Criar("Gustavo", "529.982.247-25", "g@email.com", 1000m);
        _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), default)).ReturnsAsync(cliente);

        await _handler.Handle(new SairDoProdutoCommand(1), default);

        _contaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ContaGrafica>(), default), Times.Never);
    }
}
