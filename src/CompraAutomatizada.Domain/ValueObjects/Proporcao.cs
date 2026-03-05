using CompraAutomatizada.Domain.Common;

namespace CompraAutomatizada.Domain.ValueObjects;

public record Proporcao
{
    public decimal Valor { get; }

    public Proporcao(decimal valor)
    {
        if (valor <= 0 || valor > 100)
            throw new DomainException("Proporção deve ser maior que 0 e menor que 100.");

        Valor = valor;
    }

    public override string ToString() => $"{Valor}%";
}
