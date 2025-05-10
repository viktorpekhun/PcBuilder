using PcBuilderApi.Dtos.CpuDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.CpuMappers
{
    public class CpuListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.Cpu;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var cpus = entities.Cast<Cpu>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);

            return cpus.Select(cpu =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == cpu.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;

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
                    AveragePrice = avgPrice
                };
            });
        }
    }
}
