using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PriceHistoryEntryConfiguration : IEntityTypeConfiguration<PriceHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PriceHistoryEntry> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AveragePrice)
            .HasPrecision(18, 2);

        builder.HasIndex(p => new { p.ComponentId, p.ComponentType, p.SnapshotDate })
            .IsUnique();
    }
}
