using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Components.Domain.Entities;

namespace PcBuilder.Persistence.Data.EntitiesConfigurations;

public class ProductOfferConfiguration : IEntityTypeConfiguration<ProductOffer>
{
    public void Configure(EntityTypeBuilder<ProductOffer> builder)
    {
        builder.Property(po => po.ProductOfferUrl)
            .IsRequired();

        builder.Property(po => po.Price)
            .HasPrecision(18, 2);

        builder.HasOne(po => po.Store)
            .WithMany(s => s.ProductOffers)
            .HasForeignKey(po => po.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(po => new { po.ComponentId, po.ComponentType });
    }
}
