using PcBuilderApi.Dtos.CpuCoolerDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.CpuCoolerMappers
{
    public class CpuCoolerListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.CpuCooler;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var cpuCoolers = entities.Cast<CpuCooler>();

            return cpuCoolers.Select(cpuCooler =>
            {
                var cpuCoolerSocketDtos = cpuCooler.CpuCoolerSockets
                    .Select(c => new CpuCoolerSocketDto
                    {
                        SocketType = c.SocketType
                    })
                    .ToList();
                return new CpuCoolerListDto
                {
                    Id = cpuCooler.Id,
                    Name = cpuCooler.Name,
                    PhotoUrl = cpuCooler.PhotoUrl,
                    Brand = cpuCooler.Brand,
                    Type = cpuCooler.Type,
                    FanSize = cpuCooler.FanSize,
                    RadiatorMaterial = cpuCooler.RadiatorMaterial,
                    SpeedControl = cpuCooler.SpeedControl,
                    PowerConnector = cpuCooler.PowerConnector,
                    MaxPowerDissipation = cpuCooler.MaxPowerDissipation,
                    MaxSpeed = cpuCooler.MaxSpeed,
                    NoiseLevelDb = cpuCooler.NoiseLevelDb,
                    Length = cpuCooler.Length,
                    Width = cpuCooler.Width,
                    Height = cpuCooler.Height,
                    AveragePrice = cpuCooler.AveragePrice,
                    OffersCount = cpuCooler.OffersCount,
                    CpuCoolerSockets = cpuCoolerSocketDtos
                };
            });
        }
    }
}
