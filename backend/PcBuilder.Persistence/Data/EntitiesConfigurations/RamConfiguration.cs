using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class RamConfiguration : IEntityTypeConfiguration<Ram>
{
    public void Configure(EntityTypeBuilder<Ram> builder)
    {
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Timings)
            .HasMaxLength(50);

        builder.Property(r => r.Bufferization)
            .HasMaxLength(50);

        builder.Property(r => r.AveragePrice)
            .HasPrecision(18, 2);

        builder.OwnsOne(r => r.Color, b =>
        {
            b.Property(l => l.Uk).HasColumnName("Color_Uk").HasMaxLength(100).IsRequired();
            b.Property(l => l.En).HasColumnName("Color_En").HasMaxLength(100);
        });
    }
}
