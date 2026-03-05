using CompraAutomatizada.Application.DTOs;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Admin.ConsultarCustodiaMaster;

public record ConsultarCustodiaMasterQuery : IRequest<CustodiaMasterDto>;
