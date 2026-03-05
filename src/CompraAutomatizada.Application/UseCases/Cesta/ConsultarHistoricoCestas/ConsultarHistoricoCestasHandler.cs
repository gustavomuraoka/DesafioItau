using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Cesta.ConsultarHistoricoCestas;

public class ConsultarHistoricoCestasHandler : IRequestHandler<ConsultarHistoricoCestasQuery, HistoricoCestasDto>
{
    private readonly ICestaTopFiveRepository _cestaRepository;

    public ConsultarHistoricoCestasHandler(ICestaTopFiveRepository cestaRepository)
    {
        _cestaRepository = cestaRepository;
    }

    public async Task<HistoricoCestasDto> Handle(ConsultarHistoricoCestasQuery request, CancellationToken cancellationToken)
    {
        var cestas = await _cestaRepository.GetHistoricoAsync(cancellationToken);

        var cestasDto = cestas
            .OrderByDescending(c => c.DataCriacao)
            .Select(c => new CestaDto(
                c.Id,
                c.Nome,
                c.Ativa,
                c.DataCriacao,
                c.DataDesativacao,
                c.Itens.Select(i => new ItemCestaDto(i.Ticker, i.Percentual.Valor))
            )).ToList();

        return new HistoricoCestasDto(cestasDto);
    }
}
