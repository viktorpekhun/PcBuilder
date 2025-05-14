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
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var fans = entities.Cast<Fan>();

            return fans.Select(fan =>
            {

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
                    AveragePrice = fan.AveragePrice,
                    OffersCount = fan.OffersCount,
                };
            });
        }
    }
}
