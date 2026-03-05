using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class EventoIRMapping : IEntityTypeConfiguration<EventoIR>
{
    public void Configure(EntityTypeBuilder<EventoIR> builder)
    {
        builder.ToTable("EventosIR");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ClienteId).IsRequired();
        builder.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Tipo)
            .HasConversion<string>()
            .HasMaxLength(15)
            .IsRequired();
        builder.Property(e => e.ValorOperacao).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.ValorIr).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(e => e.LucroLiquido).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PublicadoKafka).IsRequired();
        builder.Property(e => e.DataEvento).IsRequired();
    }
}
