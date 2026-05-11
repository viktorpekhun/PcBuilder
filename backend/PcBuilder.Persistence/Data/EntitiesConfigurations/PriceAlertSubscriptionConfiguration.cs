using Components.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PriceAlertSubscriptionConfiguration : IEntityTypeConfiguration<PriceAlertSubscription>
{
    public void Configure(EntityTypeBuilder<PriceAlertSubscription> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ThresholdPercent)
            .HasPrecision(5, 2);

        builder.Property(p => p.InitialPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.LastNotifiedPrice)
            .HasPrecision(18, 2);

        builder.HasIndex(p => new { p.UserId, p.ComponentId, p.ComponentType })
            .IsUnique();

        builder.HasIndex(p => new { p.ComponentId, p.ComponentType });
    }
}
