using FluentValidation;

namespace CompraAutomatizada.Application.UseCases.Motor.ExecutarCompra;

public class ExecutarCompraValidator : AbstractValidator<ExecutarCompraCommand>
{
    public ExecutarCompraValidator()
    {
        RuleFor(x => x.DataReferencia)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Data de referência não pode ser futura.");
    }
}
