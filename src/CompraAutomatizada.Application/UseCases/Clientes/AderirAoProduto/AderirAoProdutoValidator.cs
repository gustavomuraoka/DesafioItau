using FluentValidation;

namespace CompraAutomatizada.Application.UseCases.Clientes.AderirAoProduto;

public class AderirAoProdutoValidator : AbstractValidator<AderirAoProdutoCommand>
{
    public AderirAoProdutoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Length(11).WithMessage("CPF deve ter 11 dígitos.")
            .Matches(@"^\d{11}$").WithMessage("CPF deve conter apenas números.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.ValorMensal)
            .GreaterThanOrEqualTo(100).WithMessage("O valor mensal mínimo é de R$ 100,00.");
    }
}
