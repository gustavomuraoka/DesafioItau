using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Domain.Common;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Cesta.ConsultarCestaAtiva;

public class ConsultarCestaAtivaHandler : IRequestHandler<ConsultarCestaAtivaQuery, CestaAtualDto>
{
	private readonly ICestaTopFiveRepository _cestaRepository;
	private readonly ICotacaoRepository _cotacaoRepository;

	public ConsultarCestaAtivaHandler(
		ICestaTopFiveRepository cestaRepository,
		ICotacaoRepository cotacaoRepository)
	{
		_cestaRepository = cestaRepository;
		_cotacaoRepository = cotacaoRepository;
	}

	public async Task<CestaAtualDto> Handle(ConsultarCestaAtivaQuery request, CancellationToken cancellationToken)
	{
		var cesta = await _cestaRepository.GetAtivaAsync(cancellationToken)
			?? throw new DomainException("Nenhuma cesta ativa encontrada.");

		var tickers = cesta.Itens.Select(i => i.Ticker).ToList();
		var cotacoes = await _cotacaoRepository.GetUltimasByTickersAsync(tickers, cancellationToken);
		var cotacaoDict = cotacoes.ToDictionary(c => c.Ticker, c => c.PrecoFechamento);

		var itens = cesta.Itens.Select(i => new ItemCestaComCotacaoDto(
			i.Ticker,
			i.Percentual.Valor,
			cotacaoDict.GetValueOrDefault(i.Ticker, 0m)
		)).ToList();

		return new CestaAtualDto(cesta.Id, cesta.Nome, cesta.Ativa, cesta.DataCriacao, itens);
	}
}
