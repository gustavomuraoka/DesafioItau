using CompraAutomatizada.Application.DTOs;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.ConsultarCarteira;

public record ConsultarCarteiraQuery(long ClienteId) : IRequest<CarteiraDto>;
