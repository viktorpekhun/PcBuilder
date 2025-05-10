using PcBuilderApi.Dtos.RamDtos;
using PcBuilderApi.Dtos.SsdDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.SsdMappers
{
    public class SsdListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Ssd;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var ssds = entities.Cast<Ssd>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);

            return ssds.Select(ssd =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == ssd.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;

                return new SsdListDto
                {
                    Id = ssd.Id,
                    Name = ssd.Name,
                    PhotoUrl = ssd.PhotoUrl,
                    Brand = ssd.Brand,
                    Capacity = ssd.Capacity,
                    Interface = ssd.Interface,
                    NandType = ssd.NandType,
                    IsTrimmSupported = ssd.IsTrimmSupported,
                    FormFactor = ssd.FormFactor,
                    MaxReadSpeed = ssd.MaxReadSpeed,
                    MaxWriteSpeed = ssd.MaxWriteSpeed,
                    AveragePrice = avgPrice
                };
            });
        }
    }
}
