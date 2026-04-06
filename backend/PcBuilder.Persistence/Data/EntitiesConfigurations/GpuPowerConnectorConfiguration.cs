using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class GpuPowerConnectorConfiguration : IEntityTypeConfiguration<GpuPowerConnector>
{
    public void Configure(EntityTypeBuilder<GpuPowerConnector> builder)
    {
        builder.HasOne(c => c.Gpu)
            .WithMany(m => m.GpuPowerConnectors)
            .HasForeignKey(c => c.GpuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
