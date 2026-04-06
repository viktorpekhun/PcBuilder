using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PowerSupplyPowerConnectorConfiguration : IEntityTypeConfiguration<PowerSupplyPowerConnector>
{
    public void Configure(EntityTypeBuilder<PowerSupplyPowerConnector> builder)
    {
        builder.Property(p => p.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(p => p.PowerSupply)
            .WithMany(ps => ps.PowerSupplyPowerConnectors)
            .HasForeignKey(p => p.PowerSupplyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
