using PcBuilderApi.Dtos.CpuDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.CpuMappers
{
    public class CpuListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Cpu;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var cpus = entities.Cast<Cpu>();

            return cpus.Select(cpu =>
            {

                return new CpuListDto
                {
                    Id = cpu.Id,
                    Name = cpu.Name,
                    PhotoUrl = cpu.PhotoUrl,
                    Brand = cpu.Brand,
                    Socket = cpu.Socket,
                    BasicFrequency = cpu.BasicFrequency,
                    MaxFrequency = cpu.MaxFrequency,
                    Cores = cpu.Cores,
                    Threads = cpu.Threads,
                    AveragePrice = cpu.AveragePrice,
                    OffersCount = cpu.OffersCount,
                };
            });
        }
    }
}
