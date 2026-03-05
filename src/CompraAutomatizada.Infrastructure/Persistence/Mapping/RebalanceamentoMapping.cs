using CompraAutomatizada.Domain.Aggregates.RebalanceamentoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class RebalanceamentoMapping : IEntityTypeConfiguration<Rebalanceamento>
{
    public void Configure(EntityTypeBuilder<Rebalanceamento> builder)
    {
        builder.ToTable("Rebalanceamentos");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.ClienteId).IsRequired();
        builder.Property(r => r.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(r => r.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(r => r.QuantidadeVendida).IsRequired();
        builder.Property(r => r.QuantidadeComprada).IsRequired();
        builder.Property(r => r.PrecoUnitario).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(r => r.ValorTotalVendas).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.ValorIrApurado).HasColumnType("decimal(18,2)");
        builder.Property(r => r.DataRebalanceamento).IsRequired();
    }
}
