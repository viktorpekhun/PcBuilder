using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class GpuConfiguration : IEntityTypeConfiguration<Gpu>
{
    public void Configure(EntityTypeBuilder<Gpu> builder)
    {
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(g => g.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.GpuManufacturer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.GpuModel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.MemoryType)
            .HasMaxLength(20);

        builder.Property(g => g.AveragePrice)
            .HasPrecision(18, 2);

        builder.Property(g => g.PassMarkScore)
            .HasDefaultValue(0);

        builder.HasIndex(g => g.PassMarkScore);
    }
}
