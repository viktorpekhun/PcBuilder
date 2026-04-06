using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class HddConfiguration : IEntityTypeConfiguration<Hdd>
{
    public void Configure(EntityTypeBuilder<Hdd> builder)
    {
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(h => h.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.Interface)
            .HasMaxLength(50);

        builder.Property(h => h.WritingTechnology)
            .HasMaxLength(50);

        builder.Property(h => h.AveragePrice)
            .HasPrecision(18, 2);
    }
}
