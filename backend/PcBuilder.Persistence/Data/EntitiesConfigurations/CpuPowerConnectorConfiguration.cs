using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class CpuPowerConnectorConfiguration : IEntityTypeConfiguration<CpuPowerConnector>
{
    public void Configure(EntityTypeBuilder<CpuPowerConnector> builder)
    {
        builder.ToTable("MotherboardCpuPowerConnectors");

        builder.HasOne(c => c.Motherboard)
            .WithMany(m => m.CpuPowerConnectors)
            .HasForeignKey(c => c.MotherboardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
