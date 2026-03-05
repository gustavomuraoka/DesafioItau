using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Configurations;

public class ItemCestaConfiguration : IEntityTypeConfiguration<ItemCesta>
{
    public void Configure(EntityTypeBuilder<ItemCesta> builder)
    {
        builder.ToTable("ItensCesta");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Ticker).IsRequired().HasMaxLength(10);
        builder.Property(i => i.CestaId).IsRequired();

        builder.OwnsOne(i => i.Percentual, p =>
        {
            p.Property(x => x.Valor).HasColumnName("Percentual").IsRequired();
        });
    }
}
