using PcBuilderApi.Dtos.GpuDtos;
using PcBuilderApi.Dtos.MotherboardDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.MotherboardMappers
{
    public class MotherboardListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Motherboard;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var motherboards = entities.Cast<Motherboard>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);
            return motherboards.Select(motherboard =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == motherboard.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;
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
                    AveragePrice = avgPrice,
                    PcleSlots = pcleSlotDtos
                };
            });
        }
    }
}
