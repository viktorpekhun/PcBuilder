using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moderation.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.AdminResolutionNote)
            .HasMaxLength(500);

        builder.Property(r => r.ReportType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // All FKs use Restrict to avoid SQL Server's multiple-cascade-paths error.
        // AdminDeleteUserCommandHandler is responsible for deleting dependent Reports
        // before removing the User row.
        builder.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReportedUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ResolvedByAdmin)
            .WithMany()
            .HasForeignKey(r => r.ResolvedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        // No FK constraint on ReportedEntityId — discriminator pattern
        // Composite index for duplicate prevention
        builder.HasIndex(r => new { r.ReporterId, r.ReportType, r.ReportedEntityId });

        // Index for querying by status
        builder.HasIndex(r => r.Status);
    }
}
