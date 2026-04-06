using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class FanConfiguration : IEntityTypeConfiguration<Fan>
{
    public void Configure(EntityTypeBuilder<Fan> builder)
    {
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.SpeedControl)
            .HasMaxLength(50);

        builder.Property(f => f.Connector)
            .HasMaxLength(50);

        builder.Property(f => f.AveragePrice)
            .HasPrecision(18, 2);

        builder.OwnsOne(f => f.BearingType, b =>
        {
            b.Property(l => l.Uk).HasColumnName("BearingType_Uk").HasMaxLength(100).IsRequired();
            b.Property(l => l.En).HasColumnName("BearingType_En").HasMaxLength(100);
        });

        builder.OwnsOne(f => f.Color, b =>
        {
            b.Property(l => l.Uk).HasColumnName("Color_Uk").HasMaxLength(100).IsRequired();
            b.Property(l => l.En).HasColumnName("Color_En").HasMaxLength(100);
        });
    }
}
