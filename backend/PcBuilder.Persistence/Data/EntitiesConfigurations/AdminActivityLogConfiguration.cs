using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moderation.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class AdminActivityLogConfiguration : IEntityTypeConfiguration<AdminActivityLog>
{
    public void Configure(EntityTypeBuilder<AdminActivityLog> builder)
    {
        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.TargetType)
            .HasMaxLength(64);

        builder.Property(a => a.TargetName)
            .HasMaxLength(256);

        builder.Property(a => a.Detail)
            .HasMaxLength(500);

        builder.HasOne(a => a.Admin)
            .WithMany()
            .HasForeignKey(a => a.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.OccurredAt);
        builder.HasIndex(a => a.AdminId);
    }
}
