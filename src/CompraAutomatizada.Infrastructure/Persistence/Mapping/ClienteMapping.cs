using CompraAutomatizada.Domain.Aggregates.ClienteAggregate;
using CompraAutomatizada.Domain.ValueObjects;
using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class ClienteMapping : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.OwnsOne(c => c.Cpf, cpf =>
        {
            cpf.Property(v => v.Valor)
                .HasColumnName("CPF")
                .HasMaxLength(11)
                .IsRequired();
            cpf.HasIndex(v => v.Valor).IsUnique();
        });

        builder.Property(c => c.Nome).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ValorMensal).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.DataAdesao).IsRequired();

        builder.HasOne(c => c.Conta)
            .WithOne()
            .HasForeignKey<ContaGrafica>("ClienteId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
