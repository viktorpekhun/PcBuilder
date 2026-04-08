using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Text)
            .HasMaxLength(500);

        builder.HasOne(c => c.PcBuild)
            .WithMany(pb => pb.Comments)
            .HasForeignKey(c => c.PcBuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}