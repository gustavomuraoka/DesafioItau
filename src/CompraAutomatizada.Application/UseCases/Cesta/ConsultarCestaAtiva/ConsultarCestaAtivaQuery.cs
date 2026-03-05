using CompraAutomatizada.Application.DTOs;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Cesta.ConsultarCestaAtiva;

public record ConsultarCestaAtivaQuery : IRequest<CestaAtualDto>;
