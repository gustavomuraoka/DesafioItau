using CompraAutomatizada.Domain.Aggregates.OrdemAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class DistribuicaoMapping : IEntityTypeConfiguration<Distribuicao>
{
    public void Configure(EntityTypeBuilder<Distribuicao> builder)
    {
        builder.ToTable("Distribuicoes");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();

        builder.Property(d => d.OrdemCompraId).IsRequired();
        builder.Property(d => d.CustodiaFilhoteId).IsRequired();
        builder.Property(d => d.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(d => d.Quantidade).IsRequired();
        builder.Property(d => d.PrecoUnitario).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(d => d.ValorIrDedoDuro).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(d => d.DataDistribuicao).IsRequired();
    }
}
