using FluentValidation;

namespace CompraAutomatizada.Application.UseCases.Clientes.AlterarValorMensal;

public class AlterarValorMensalValidator : AbstractValidator<AlterarValorMensalCommand>
{
    public AlterarValorMensalValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0).WithMessage("ClienteId inválido.");

        RuleFor(x => x.NovoValorMensal)
            .GreaterThanOrEqualTo(100).WithMessage("O valor mensal mínimo é de R$ 100,00.");
    }
}
