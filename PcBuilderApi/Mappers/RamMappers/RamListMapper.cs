using PcBuilderApi.Dtos.RamDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.RamMappers
{
    public class RamListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Ram;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var rams = entities.Cast<Ram>();

            return rams.Select(ram =>
            {

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
                    AveragePrice = ram.AveragePrice,
                    OffersCount = ram.OffersCount,
                };
            });
        }
    }
}
