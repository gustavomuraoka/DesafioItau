using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Admin.ConsultarCustodiaMaster;

public class ConsultarCustodiaMasterHandler : IRequestHandler<ConsultarCustodiaMasterQuery, CustodiaMasterDto>
{
    private readonly IContaGraficaRepository _contaRepository;
    private readonly ICotacaoRepository _cotacaoRepository;

    public ConsultarCustodiaMasterHandler(
        IContaGraficaRepository contaRepository,
        ICotacaoRepository cotacaoRepository)
    {
        _contaRepository = contaRepository;
        _cotacaoRepository = cotacaoRepository;
    }

    public async Task<CustodiaMasterDto> Handle(ConsultarCustodiaMasterQuery request, CancellationToken cancellationToken)
    {
        var master = await _contaRepository.GetMasterAsync(cancellationToken)
            ?? throw new DomainException("Conta master não encontrada.");

        var tickers = master.Posicoes.Select(p => p.Ticker).ToList();
        var cotacoes = await _cotacaoRepository.GetUltimasByTickersAsync(tickers, cancellationToken);
        var cotacaoDict = cotacoes.ToDictionary(c => c.Ticker, c => c.PrecoFechamento);

        var posicoes = master.Posicoes.Select(p =>
        {
            var cotacaoAtual = cotacaoDict.GetValueOrDefault(p.Ticker, p.PrecoMedio);
            return new PosicaoMasterDto(
                p.Ticker,
                p.Quantidade,
                p.PrecoMedio,
                Math.Round(p.Quantidade * cotacaoAtual, 2),
                $"Resíduo distribuição {p.DataUltimaAtualizacao:yyyy-MM-dd}"
            );
        }).ToList();

        var valorTotal = posicoes.Sum(p => p.ValorAtual);

        return new CustodiaMasterDto(
            new ContaMasterInfoDto(master.Id, master.NumeroConta, master.Tipo.ToString().ToUpper()),
            posicoes,
            Math.Round(valorTotal, 2)
        );
    }
}
