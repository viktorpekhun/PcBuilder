using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class CpuConfiguration : IEntityTypeConfiguration<Cpu>
{
    public void Configure(EntityTypeBuilder<Cpu> builder)
    {
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Socket)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.DimmType)
            .HasMaxLength(50);

        builder.Property(c => c.Techprocess)
            .HasMaxLength(50);

        builder.Property(c => c.Complectation)
            .HasMaxLength(255);

        builder.Property(c => c.AveragePrice)
            .HasPrecision(18, 2);
    }
}
