using CompraAutomatizada.Application.DTOs;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Clientes.ConsultarRentabilidade;

public record ConsultarRentabilidadeQuery(long ClienteId) : IRequest<RentabilidadeDto>;
