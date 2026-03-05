using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class OrdemDeCompraMapping : IEntityTypeConfiguration<OrdemDeCompra>
{
    public void Configure(EntityTypeBuilder<OrdemDeCompra> builder)
    {
        builder.ToTable("OrdensCompra");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.ContaMasterId).IsRequired();
        builder.Property(o => o.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(o => o.Quantidade).IsRequired();
        builder.Property(o => o.PrecoUnitario).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(o => o.TipoMercado)
            .HasConversion<string>()
            .HasMaxLength(15)
            .IsRequired();
        builder.Property(o => o.DataExecucao).IsRequired();

        builder.HasMany(o => o.Distribuicoes)
            .WithOne()
            .HasForeignKey(d => d.OrdemCompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
