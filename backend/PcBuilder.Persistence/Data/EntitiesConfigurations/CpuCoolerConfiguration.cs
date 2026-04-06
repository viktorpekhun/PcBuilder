using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class CpuCoolerConfiguration : IEntityTypeConfiguration<CpuCooler>
{
    public void Configure(EntityTypeBuilder<CpuCooler> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CHK_CpuCooler_Type", "Type IN ('Air', 'Water')"));

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.SpeedControl)
            .HasMaxLength(100);

        builder.Property(c => c.PowerConnector)
            .HasMaxLength(100);

        builder.Property(c => c.AveragePrice)
            .HasPrecision(18, 2);

        builder.OwnsOne(c => c.RadiatorMaterial, b =>
        {
            b.Property(l => l.Uk).HasColumnName("RadiatorMaterial_Uk").HasMaxLength(100).IsRequired();
            b.Property(l => l.En).HasColumnName("RadiatorMaterial_En").HasMaxLength(100);
        });
    }
}
