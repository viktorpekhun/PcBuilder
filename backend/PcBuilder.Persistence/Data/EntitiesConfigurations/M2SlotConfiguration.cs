using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class M2SlotConfiguration : IEntityTypeConfiguration<M2Slot>
{
    public void Configure(EntityTypeBuilder<M2Slot> builder)
    {
        builder.ToTable("MotherboardM2Slots");

        builder.HasOne(m => m.Motherboard)
            .WithMany(mb => mb.M2Slots)
            .HasForeignKey(m => m.MotherboardId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
