using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Configurations;

public class CestaTopFiveConfiguration : IEntityTypeConfiguration<CestaTopFive>
{
    public void Configure(EntityTypeBuilder<CestaTopFive> builder)
    {
        builder.ToTable("CestasRecomendacao");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Ativa).IsRequired();
        builder.Property(c => c.DataCriacao).IsRequired();
        builder.Property(c => c.DataDesativacao);

        builder.Navigation(c => c.Itens).HasField("_itens");

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CestaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
