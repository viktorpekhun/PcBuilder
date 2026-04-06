using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class MotherboardConfiguration : IEntityTypeConfiguration<Motherboard>
{
    public void Configure(EntityTypeBuilder<Motherboard> builder)
    {
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Socket)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Chipset)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.DimmType)
            .HasMaxLength(20);

        builder.Property(m => m.Ethernet)
            .HasMaxLength(100);

        builder.Property(m => m.Audio)
            .HasMaxLength(100);

        builder.Property(m => m.Wifi)
            .HasMaxLength(100);

        builder.Property(m => m.Bluetooth)
            .HasMaxLength(100);

        builder.Property(m => m.VideoPorts)
            .HasMaxLength(255);

        builder.Property(m => m.FormFactor)
            .HasMaxLength(50);

        builder.Property(m => m.SizeDimentions)
            .HasMaxLength(50);

        builder.Property(m => m.AveragePrice)
            .HasPrecision(18, 2);
    }
}
