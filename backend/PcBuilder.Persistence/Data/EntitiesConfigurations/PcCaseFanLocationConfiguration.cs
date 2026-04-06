using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcCaseFanLocationConfiguration : IEntityTypeConfiguration<PcCaseFanLocation>
{
    public void Configure(EntityTypeBuilder<PcCaseFanLocation> builder)
    {
        builder.HasOne(cf => cf.PcCase)
            .WithMany(c => c.PcCaseFanLocations)
            .HasForeignKey(cf => cf.PcCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(p => p.Name, b =>
        {
            b.Property(l => l.Uk).HasColumnName("Name_Uk").HasMaxLength(50).IsRequired();
            b.Property(l => l.En).HasColumnName("Name_En").HasMaxLength(50);
        });
    }
}
