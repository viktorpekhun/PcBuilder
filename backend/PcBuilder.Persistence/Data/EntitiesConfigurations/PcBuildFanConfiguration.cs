using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcBuildFanConfiguration : IEntityTypeConfiguration<PcBuild_Fan>
{
    public void Configure(EntityTypeBuilder<PcBuild_Fan> builder)
    {
        builder.ToTable("PcBuild_Fan");

        builder.HasOne(p => p.PcBuild)
            .WithMany(pb => pb.PcBuild_Fans)
            .HasForeignKey(p => p.PcBuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Fan)
            .WithMany()
            .HasForeignKey(p => p.FanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductOffer)
            .WithMany()
            .HasForeignKey(p => p.ProductOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
