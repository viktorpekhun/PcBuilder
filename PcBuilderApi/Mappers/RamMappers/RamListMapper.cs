using PcBuilderApi.Dtos.RamDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.RamMappers
{
    public class RamListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Ram;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var rams = entities.Cast<Ram>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);

            return rams.Select(ram =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == ram.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;

                return new RamListDto
                {
                    Id = ram.Id,
                    Name = ram.Name,
                    PhotoUrl = ram.PhotoUrl,
                    Brand = ram.Brand,
                    Type = ram.Type,
                    Frequency = ram.Frequency,
                    Capacity = ram.Capacity,
                    Timings = ram.Timings,
                    Voltage = ram.Voltage,
                    Xmp = ram.Xmp,
                    AveragePrice = avgPrice
                };
            });
        }
    }
}
