using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PowerSupplyConfiguration : IEntityTypeConfiguration<PowerSupply>
{
    public void Configure(EntityTypeBuilder<PowerSupply> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FormFactor)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.EfficiencyStandart)
            .HasMaxLength(50);

        builder.Property(p => p.Size)
            .HasMaxLength(50);

        builder.Property(p => p.AveragePrice)
            .HasPrecision(18, 2);

        builder.OwnsOne(p => p.Modularity, b =>
        {
            b.Property(l => l.Uk).HasColumnName("Modularity_Uk").HasMaxLength(100).IsRequired();
            b.Property(l => l.En).HasColumnName("Modularity_En").HasMaxLength(100);
        });
    }
}
