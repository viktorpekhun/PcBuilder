using PcBuilderApi.Dtos.GpuDtos;
using PcBuilderApi.Dtos.MotherboardDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.MotherboardMappers
{
    public class MotherboardListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Motherboard;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var motherboards = entities.Cast<Motherboard>();
            return motherboards.Select(motherboard =>
            {
                var pcleSlotDtos = motherboard.PcleSlots
                    .Select(c => new PcleSlotDto
                    {
                        Version = c.Version,
                        Lane = c.Lane,
                        Quantity = c.Quantity
                    })
                    .ToList();
                return new MotherboardListDto
                {
                    Id = motherboard.Id,
                    Name = motherboard.Name,
                    PhotoUrl = motherboard.PhotoUrl,
                    Brand = motherboard.Brand,
                    Socket = motherboard.Socket,
                    Chipset = motherboard.Chipset,
                    DimmSlots = motherboard.DimmSlots,
                    DimmType = motherboard.DimmType,
                    DimmFrequency = motherboard.DimmFrequency,
                    DimmCapacity = motherboard.DimmCapacity,
                    FormFactor = motherboard.FormFactor,
                    SizeDimentions = motherboard.SizeDimentions,
                    AveragePrice = motherboard.AveragePrice,
                    OffersCount = motherboard.OffersCount,
                    PcleSlots = pcleSlotDtos
                };
            });
        }
    }
}
