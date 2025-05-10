using PcBuilderApi.Dtos.HddDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.HddMappers
{
    public class HddListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Hdd;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var hdds = entities.Cast<Hdd>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);

            return hdds.Select(hdd =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == hdd.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;

                return new HddListDto
                {
                    Id = hdd.Id,
                    Name = hdd.Name,
                    PhotoUrl = hdd.PhotoUrl,
                    Brand = hdd.Brand,
                    Capacity = hdd.Capacity,
                    Interface = hdd.Interface,
                    FormFactor = hdd.FormFactor,
                    SpindleSpeed = hdd.SpindleSpeed,
                    Cache = hdd.Cache,
                    AveragePrice = avgPrice
                };
            });
        }
    }
}
