using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.ConsultarCarteira;

public class ConsultarCarteiraHandler : IRequestHandler<ConsultarCarteiraQuery, CarteiraDto>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICotacaoRepository _cotacaoRepository;

    public ConsultarCarteiraHandler(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICotacaoRepository cotacaoRepository)
    {
        _clienteRepository = clienteRepository;
        _contaGraficaRepository = contaGraficaRepository;
        _cotacaoRepository = cotacaoRepository;
    }

    public async Task<CarteiraDto> Handle(ConsultarCarteiraQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken)
            ?? throw new DomainException("Cliente não encontrado.");

        var conta = await _contaGraficaRepository.GetByClienteIdAsync(request.ClienteId, cancellationToken)
            ?? throw new DomainException("Conta gráfica não encontrada.");

        var tickers = conta.Posicoes.Select(p => p.Ticker).ToList();
        var cotacoes = await _cotacaoRepository.GetUltimasByTickersAsync(tickers, cancellationToken);
        var cotacaoDict = cotacoes.ToDictionary(c => c.Ticker, c => c.PrecoFechamento);

        var ativos = conta.Posicoes.Select(posicao =>
        {
            var cotacaoAtual = cotacaoDict.GetValueOrDefault(posicao.Ticker, posicao.PrecoMedio);
            var valorAtual = posicao.Quantidade * cotacaoAtual;
            var valorInvestido = posicao.Quantidade * posicao.PrecoMedio;
            var pl = valorAtual - valorInvestido;
            var plPercentual = valorInvestido > 0 ? Math.Round(pl / valorInvestido * 100, 2) : 0m;

            return new { posicao, cotacaoAtual, valorAtual, valorInvestido, pl, plPercentual };
        }).ToList();

        var valorTotalAtual = ativos.Sum(a => a.valorAtual);
        var valorTotalInvestido = ativos.Sum(a => a.valorInvestido);
        var plTotal = valorTotalAtual - valorTotalInvestido;
        var rentabilidadeTotal = valorTotalInvestido > 0
            ? Math.Round(plTotal / valorTotalInvestido * 100, 2)
            : 0m;

        var ativosDto = ativos.Select(a => new AtivoCarteiraDto(
            a.posicao.Ticker,
            a.posicao.Quantidade,
            a.posicao.PrecoMedio,
            a.cotacaoAtual,
            Math.Round(a.valorAtual, 2),
            Math.Round(a.pl, 2),
            a.plPercentual,
            valorTotalAtual > 0 ? Math.Round(a.valorAtual / valorTotalAtual * 100, 2) : 0m
        )).ToList();

        return new CarteiraDto(
            cliente.Id,
            cliente.Nome,
            conta.NumeroConta,
            DateTime.UtcNow,
            new ResumoCarteiraDto(
                Math.Round(valorTotalInvestido, 2),
                Math.Round(valorTotalAtual, 2),
                Math.Round(plTotal, 2),
                rentabilidadeTotal
            ),
            ativosDto
        );
    }
}
