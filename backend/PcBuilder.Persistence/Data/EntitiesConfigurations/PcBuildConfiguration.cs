using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcBuildConfiguration : IEntityTypeConfiguration<PcBuild>
{
    public void Configure(EntityTypeBuilder<PcBuild> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.AverageRating)
            .HasPrecision(3, 2);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Cpu)
            .WithMany()
            .HasForeignKey(p => p.CpuId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.CpuOffer)
            .WithMany()
            .HasForeignKey(p => p.CpuOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.Gpu)
            .WithMany()
            .HasForeignKey(p => p.GpuId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.GpuOffer)
            .WithMany()
            .HasForeignKey(p => p.GpuOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.Motherboard)
            .WithMany()
            .HasForeignKey(p => p.MotherboardId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.MotherboardOffer)
            .WithMany()
            .HasForeignKey(p => p.MotherboardOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.CpuCooler)
            .WithMany()
            .HasForeignKey(p => p.CpuCoolerId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.CpuCoolerOffer)
            .WithMany()
            .HasForeignKey(p => p.CpuCoolerOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.PowerSupply)
            .WithMany()
            .HasForeignKey(p => p.PowerSupplyId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.PowerSupplyOffer)
            .WithMany()
            .HasForeignKey(p => p.PowerSupplyOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.PcCase)
            .WithMany()
            .HasForeignKey(p => p.PcCaseId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(p => p.PcCaseOffer)
            .WithMany()
            .HasForeignKey(p => p.PcCaseOfferId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
