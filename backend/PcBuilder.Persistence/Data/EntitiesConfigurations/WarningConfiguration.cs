using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moderation.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class WarningConfiguration : IEntityTypeConfiguration<Warning>
{
    public void Configure(EntityTypeBuilder<Warning> builder)
    {
        builder.Property(w => w.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.BanType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Restrict on both FKs to avoid SQL Server multiple-cascade-paths error.
        // AdminDeleteUserCommandHandler deletes Warnings before removing the User.
        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.IssuedByAdmin)
            .WithMany()
            .HasForeignKey(w => w.IssuedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for escalation query: user + ban type + recent date
        builder.HasIndex(w => new { w.UserId, w.IssuedAt });
    }
}
