using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class CotacaoMapping : IEntityTypeConfiguration<Cotacao>
{
    public void Configure(EntityTypeBuilder<Cotacao> builder)
    {
        builder.ToTable("Cotacoes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(c => c.DataPregao).IsRequired();
        builder.Property(c => c.PrecoAbertura).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.PrecoFechamento).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.PrecoMaximo).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.PrecoMinimo).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.VolumeNegociado).IsRequired();

        builder.HasIndex(c => new { c.Ticker, c.DataPregao }).IsUnique();
    }
}
