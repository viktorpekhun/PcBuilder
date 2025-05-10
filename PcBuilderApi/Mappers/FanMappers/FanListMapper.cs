using PcBuilderApi.Dtos.CpuDtos;
using PcBuilderApi.Dtos.FanDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.FanMappers
{
    public class FanListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Fan;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var fans = entities.Cast<Fan>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);

            return fans.Select(fan =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == fan.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;

                return new FanListDto
                {
                    Id = fan.Id,
                    Name = fan.Name,
                    PhotoUrl = fan.PhotoUrl,
                    Brand = fan.Brand,
                    ModuleCount = fan.ModuleCount,
                    BearingType = fan.BearingType,
                    SpeedControl = fan.SpeedControl,
                    Connector = fan.Connector,
                    Color = fan.Color,
                    MaxSpeed = fan.MaxSpeed,
                    NoiseLevelDb = fan.NoiseLevelDb,
                    SizeLength = fan.SizeLength,
                    SizeWidth = fan.SizeWidth,
                    SizeHeight = fan.SizeHeight,
                    AveragePrice = avgPrice
                };
            });
        }
    }
}
