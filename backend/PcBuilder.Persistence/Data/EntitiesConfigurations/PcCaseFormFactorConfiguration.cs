using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class PcCaseFormFactorConfiguration : IEntityTypeConfiguration<PcCaseFormFactor>
{
    public void Configure(EntityTypeBuilder<PcCaseFormFactor> builder)
    {
        builder.Property(cf => cf.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasOne(cf => cf.PcCase)
            .WithMany(c => c.PcCaseFormFactors)
            .HasForeignKey(cf => cf.PcCaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
