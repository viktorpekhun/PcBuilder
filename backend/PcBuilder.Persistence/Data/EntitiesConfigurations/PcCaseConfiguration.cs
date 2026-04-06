using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcCaseConfiguration : IEntityTypeConfiguration<PcCase>
{
    public void Configure(EntityTypeBuilder<PcCase> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.SizeStandard)
            .HasMaxLength(100);

        builder.Property(p => p.SizeDimentions)
            .HasMaxLength(100);

        builder.Property(p => p.Usb)
            .HasMaxLength(100);

        builder.Property(p => p.AveragePrice)
            .HasPrecision(18, 2);

        builder.OwnsOne(p => p.PsuLocation, b =>
        {
            b.Property(l => l.Uk).HasColumnName("PsuLocation_Uk").HasMaxLength(100).IsRequired();
            b.Property(l => l.En).HasColumnName("PsuLocation_En").HasMaxLength(100);
        });

        builder.OwnsOne(p => p.BuiltInFans, b =>
        {
            b.Property(l => l.Uk).HasColumnName("BuiltInFans_Uk").HasMaxLength(200).IsRequired();
            b.Property(l => l.En).HasColumnName("BuiltInFans_En").HasMaxLength(200);
        });

        builder.OwnsOne(p => p.AdditionalFanPlaces, b =>
        {
            b.Property(l => l.Uk).HasColumnName("AdditionalFanPlaces_Uk").HasMaxLength(200).IsRequired();
            b.Property(l => l.En).HasColumnName("AdditionalFanPlaces_En").HasMaxLength(200);
        });
    }
}
