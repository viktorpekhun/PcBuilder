using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcBuildHddConfiguration : IEntityTypeConfiguration<PcBuild_Hdd>
{
    public void Configure(EntityTypeBuilder<PcBuild_Hdd> builder)
    {
        builder.ToTable("PcBuild_Hdd");

        builder.HasOne(p => p.PcBuild)
            .WithMany(pb => pb.PcBuild_Hdds)
            .HasForeignKey(p => p.PcBuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Hdd)
            .WithMany()
            .HasForeignKey(p => p.HddId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductOffer)
            .WithMany()
            .HasForeignKey(p => p.ProductOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
