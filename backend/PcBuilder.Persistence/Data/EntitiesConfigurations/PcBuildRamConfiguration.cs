using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcBuildRamConfiguration : IEntityTypeConfiguration<PcBuild_Ram>
{
    public void Configure(EntityTypeBuilder<PcBuild_Ram> builder)
    {
        builder.ToTable("PcBuild_Ram");

        builder.HasOne(p => p.PcBuild)
            .WithMany(pb => pb.PcBuild_Rams)
            .HasForeignKey(p => p.PcBuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Ram)
            .WithMany()
            .HasForeignKey(p => p.RamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductOffer)
            .WithMany()
            .HasForeignKey(p => p.ProductOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
