using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class ItemCestaMapping : IEntityTypeConfiguration<ItemCesta>
{
    public void Configure(EntityTypeBuilder<ItemCesta> builder)
    {
        builder.ToTable("ItensCesta");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.CestaId).IsRequired();
        builder.Property(i => i.Ticker).HasMaxLength(10).IsRequired();

        builder.OwnsOne(i => i.Percentual, p =>
        {
            p.Property(v => v.Valor)
                .HasColumnName("Percentual")
                .HasColumnType("decimal(5,2)")
                .IsRequired();
        });
    }
}
