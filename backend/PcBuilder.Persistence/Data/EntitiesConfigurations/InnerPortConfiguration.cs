using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class InnerPortConfiguration : IEntityTypeConfiguration<InnerPort>
{
    public void Configure(EntityTypeBuilder<InnerPort> builder)
    {
        builder.ToTable("MotherboardInnerPorts");

        builder.Property(i => i.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Value)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasOne(i => i.Motherboard)
            .WithMany(m => m.InnerPorts)
            .HasForeignKey(i => i.MotherboardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
