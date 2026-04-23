using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilder.Persistence.Data.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class ScrapeJobConfiguration : IEntityTypeConfiguration<ScrapeJob>
{
    public void Configure(EntityTypeBuilder<ScrapeJob> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.ComponentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.State)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(j => j.Kind)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Category");

        builder.Property(j => j.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(j => j.ItemsScraped)
            .HasDefaultValue(0);

        builder.HasIndex(j => new { j.ComponentType, j.State });
        builder.HasIndex(j => j.QueuedAt);
    }
}
