using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class RearPortConfiguration : IEntityTypeConfiguration<RearPort>
{
    public void Configure(EntityTypeBuilder<RearPort> builder)
    {
        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(r => r.Motherboard)
            .WithMany(m => m.RearPorts)
            .HasForeignKey(r => r.MotherboardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
