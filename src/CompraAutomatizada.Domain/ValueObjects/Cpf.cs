using CompraAutomatizada.Domain.Common;

namespace CompraAutomatizada.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    public Cpf(string valor)
    {
        var digits = new string(valor.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new DomainException("CPF deve conter 11 dígitos.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CPF inválido.");

        if (!ValidarDigitosVerificadores(digits))
            throw new DomainException("CPF inválido.");

        Valor = digits;
    }

    private static bool ValidarDigitosVerificadores(string cpf)
    {
        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += int.Parse(cpf[i].ToString()) * (10 - i);
        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;
        if (digito1 != int.Parse(cpf[9].ToString())) return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(cpf[i].ToString()) * (11 - i);
        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;
        return digito2 == int.Parse(cpf[10].ToString());
    }

    public override string ToString() => Valor;
}
