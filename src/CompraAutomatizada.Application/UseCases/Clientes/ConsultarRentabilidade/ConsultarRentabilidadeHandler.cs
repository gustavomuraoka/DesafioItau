using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.ConsultarRentabilidade;

public class ConsultarRentabilidadeHandler : IRequestHandler<ConsultarRentabilidadeQuery, RentabilidadeDto>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICotacaoRepository _cotacaoRepository;
    private readonly IOrdemDeCompraRepository _ordemRepository;

    public ConsultarRentabilidadeHandler(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICotacaoRepository cotacaoRepository,
        IOrdemDeCompraRepository ordemRepository)
    {
        _clienteRepository = clienteRepository;
        _contaGraficaRepository = contaGraficaRepository;
        _cotacaoRepository = cotacaoRepository;
        _ordemRepository = ordemRepository;
    }

    public async Task<RentabilidadeDto> Handle(ConsultarRentabilidadeQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken)
            ?? throw new DomainException("Cliente não encontrado.");

        var conta = await _contaGraficaRepository.GetByClienteIdAsync(request.ClienteId, cancellationToken)
            ?? throw new DomainException("Conta gráfica não encontrada.");

        var tickers = conta.Posicoes.Select(p => p.Ticker).ToList();
        var cotacoes = await _cotacaoRepository.GetUltimasByTickersAsync(tickers, cancellationToken);
        var cotacaoDict = cotacoes.ToDictionary(c => c.Ticker, c => c.PrecoFechamento);

        var valorTotalAtual = conta.Posicoes.Sum(p =>
            p.Quantidade * cotacaoDict.GetValueOrDefault(p.Ticker, p.PrecoMedio));
        var valorTotalInvestido = conta.Posicoes.Sum(p => p.Quantidade * p.PrecoMedio);
        var plTotal = valorTotalAtual - valorTotalInvestido;
        var rentabilidade = valorTotalInvestido > 0
            ? Math.Round(plTotal / valorTotalInvestido * 100, 2)
            : 0m;

        var distribuicoes = await _ordemRepository.GetDistribuicoesByClienteIdAsync(request.ClienteId, cancellationToken);

        if (!distribuicoes.Any())
        {
            return new RentabilidadeDto(
                cliente.Id,
                cliente.Nome,
                DateTime.UtcNow,
                new ResumoRentabilidadeDto(0, 0, 0, 0),
                new List<HistoricoAporteDto>(),
                new List<EvolucaoCarteiraDto>()
            );
        }

        var parcelasPorData = distribuicoes
            .GroupBy(d => DateOnly.FromDateTime(d.DataDistribuicao))
            .OrderBy(g => g.Key)
            .ToList();

        var totalParcelas = parcelasPorData.Count;
        var historicoAportes = parcelasPorData
            .Select((g, i) => new HistoricoAporteDto(
                g.Key,
                Math.Round(g.Sum(d => d.ValorTotal), 2),
                $"{i + 1}/{totalParcelas}"
            )).ToList();

        var evolucao = parcelasPorData
            .Select((g, i) =>
            {
                var valorInvestidoAcumulado = parcelasPorData
                    .Take(i + 1)
                    .Sum(pg => pg.Sum(d => d.ValorTotal));

                var proporcao = valorTotalInvestido > 0
                    ? valorInvestidoAcumulado / valorTotalInvestido
                    : 1m;
                var valorCarteiraEstimado = Math.Round(valorTotalAtual * proporcao, 2);
                var rentabilidadeParcial = valorInvestidoAcumulado > 0
                    ? Math.Round((valorCarteiraEstimado - valorInvestidoAcumulado) / valorInvestidoAcumulado * 100, 2)
                    : 0m;

                return new EvolucaoCarteiraDto(
                    g.Key,
                    valorCarteiraEstimado,
                    Math.Round(valorInvestidoAcumulado, 2),
                    rentabilidadeParcial
                );
            }).ToList();

        return new RentabilidadeDto(
            cliente.Id,
            cliente.Nome,
            DateTime.UtcNow,
            new ResumoRentabilidadeDto(
                Math.Round(valorTotalInvestido, 2),
                Math.Round(valorTotalAtual, 2),
                Math.Round(plTotal, 2),
                rentabilidade
            ),
            historicoAportes,
            evolucao
        );
    }
}
