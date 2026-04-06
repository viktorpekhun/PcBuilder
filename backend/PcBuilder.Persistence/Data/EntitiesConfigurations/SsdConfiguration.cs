using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class SsdConfiguration : IEntityTypeConfiguration<Ssd>
{
    public void Configure(EntityTypeBuilder<Ssd> builder)
    {
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Interface)
            .HasMaxLength(300);

        builder.Property(s => s.NandType)
            .HasMaxLength(50);

        builder.Property(s => s.FormFactor)
            .HasMaxLength(50);

        builder.Property(s => s.Size)
            .HasMaxLength(50);

        builder.Property(s => s.AveragePrice)
            .HasPrecision(18, 2);
    }
}
