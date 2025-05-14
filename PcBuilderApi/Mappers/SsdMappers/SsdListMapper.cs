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
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var ssds = entities.Cast<Ssd>();

            return ssds.Select(ssd =>
            {

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
                    AveragePrice = ssd.AveragePrice,
                    OffersCount = ssd.OffersCount,
                };
            });
        }
    }
}
