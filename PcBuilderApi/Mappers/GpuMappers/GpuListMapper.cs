using PcBuilderApi.Dtos.GpuDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.GpuMappers
{
    public class GpuListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Gpu;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var gpus = entities.Cast<Gpu>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);

            return gpus.Select(gpu =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == gpu.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;

                return new GpuListDto
                {
                    Id = gpu.Id,
                    Name = gpu.Name,
                    PhotoUrl = gpu.PhotoUrl,
                    Brand = gpu.Brand,
                    GpuManufacturer = gpu.GpuManufacturer,
                    Memory = gpu.Memory,
                    MemoryType = gpu.MemoryType,
                    PcleVersion = gpu.PcleVersion,
                    PcleLane = gpu.PcleLane,
                    MaxFrequency = gpu.MaxFrequency,
                    AveragePrice = avgPrice
                };
            });
        }
    }
}
