using PcBuilderApi.Dtos.GpuDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.GpuMappers
{
    public class GpuListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Gpu;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var gpus = entities.Cast<Gpu>();

            return gpus.Select(gpu =>
            {

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
                    AveragePrice = gpu.AveragePrice,
                    OffersCount = gpu.OffersCount,
                };
            });
        }
    }
}
