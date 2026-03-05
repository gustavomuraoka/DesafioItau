using CompraAutomatizada.Domain.Aggregates.ContaGraficaAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class CustodiaMapping : IEntityTypeConfiguration<Custodia>
{
    public void Configure(EntityTypeBuilder<Custodia> builder)
    {
        builder.ToTable("Custodias");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.ContaGraficaId).IsRequired();
        builder.Property(c => c.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(c => c.Quantidade).IsRequired();
        builder.Property(c => c.PrecoMedio).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.DataUltimaAtualizacao).IsRequired();

        builder.HasIndex(c => new { c.ContaGraficaId, c.Ticker }).IsUnique();
    }
}
