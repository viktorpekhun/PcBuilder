using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcBuildSsdConfiguration : IEntityTypeConfiguration<PcBuild_Ssd>
{
    public void Configure(EntityTypeBuilder<PcBuild_Ssd> builder)
    {
        builder.ToTable("PcBuild_Ssd");

        builder.HasOne(p => p.PcBuild)
            .WithMany(pb => pb.PcBuild_Ssds)
            .HasForeignKey(p => p.PcBuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Ssd)
            .WithMany()
            .HasForeignKey(p => p.SsdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductOffer)
            .WithMany()
            .HasForeignKey(p => p.ProductOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
