using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompraAutomatizada.Infrastructure.Persistence.Mappings;

public class CestaTopFiveMapping : IEntityTypeConfiguration<CestaTopFive>
{
    public void Configure(EntityTypeBuilder<CestaTopFive> builder)
    {
        builder.ToTable("CestasRecomendacao");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Nome).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Ativa).IsRequired();
        builder.Property(c => c.DataCriacao).IsRequired();
        builder.Property(c => c.DataDesativacao);

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CestaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
