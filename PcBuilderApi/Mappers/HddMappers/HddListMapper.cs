using PcBuilderApi.Dtos.HddDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.HddMappers
{
    public class HddListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Hdd;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var hdds = entities.Cast<Hdd>();

            return hdds.Select(hdd =>
            {

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
                    AveragePrice = hdd.AveragePrice,
                    OffersCount = hdd.OffersCount,
                };
            });
        }
    }
}
