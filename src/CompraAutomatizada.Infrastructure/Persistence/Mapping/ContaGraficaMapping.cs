using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class ContaGraficaMapping : IEntityTypeConfiguration<ContaGrafica>
{
    public void Configure(EntityTypeBuilder<ContaGrafica> builder)
    {
        builder.ToTable("ContasGraficas");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.NumeroConta).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.NumeroConta).IsUnique();

        builder.Property(c => c.Tipo)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.DataCriacao).IsRequired();

        builder.HasMany(c => c.Posicoes)
            .WithOne()
            .HasForeignKey(p => p.ContaGraficaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
