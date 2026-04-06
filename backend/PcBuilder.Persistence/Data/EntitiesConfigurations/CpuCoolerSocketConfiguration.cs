using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class CpuCoolerSocketConfiguration : IEntityTypeConfiguration<CpuCoolerSocket>
{
    public void Configure(EntityTypeBuilder<CpuCoolerSocket> builder)
    {
        builder.Property(c => c.SocketType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(c => c.CpuCooler)
            .WithMany(m => m.CpuCoolerSockets)
            .HasForeignKey(c => c.CpuCoolerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
