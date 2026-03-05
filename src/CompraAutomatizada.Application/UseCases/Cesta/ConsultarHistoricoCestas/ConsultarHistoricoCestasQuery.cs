using CompraAutomatizada.Application.DTOs;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Cesta.ConsultarHistoricoCestas;

public record ConsultarHistoricoCestasQuery : IRequest<HistoricoCestasDto>;
