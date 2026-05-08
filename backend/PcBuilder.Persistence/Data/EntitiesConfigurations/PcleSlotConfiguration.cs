using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcleSlotConfiguration : IEntityTypeConfiguration<PcleSlot>
{
    public void Configure(EntityTypeBuilder<PcleSlot> builder)
    {
        builder.ToTable("MotherboardPcleSlots");

        builder.HasOne(p => p.Motherboard)
            .WithMany(m => m.PcleSlots)
            .HasForeignKey(p => p.MotherboardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
