using PcBuilderApi.Dtos.PcBuildDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Compatibility;
using PcBuilderApi.Services.Interfaces;
using PcBuilderApi.Utilities;
using System.Runtime.Intrinsics.X86;

namespace PcBuilderApi.Services.Implementations
{
    public class PcBuildService : IPcBuildService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CompatibilityChecker _compatibilityChecker;

        public PcBuildService(IUnitOfWork unitOfWork, CompatibilityChecker compatibilityChecker)
        {
            _unitOfWork = unitOfWork;
            _compatibilityChecker = compatibilityChecker;
        }

        public async Task<List<CompatibilityResult>> CheckComponentsCompatibilityAsync(ComponentsCompatibilityDto dto)
        {

            var tempBuild = new PcBuild();

            if (dto.CpuId.HasValue)
                tempBuild.Cpu = await _unitOfWork.Repository<Cpu>().GetFirstOrDefaultAsync(c => c.Id == dto.CpuId.Value);

            if (dto.MotherboardId.HasValue)
                tempBuild.Motherboard = await _unitOfWork.Repository<Motherboard>().GetFirstOrDefaultAsync(m => m.Id == dto.MotherboardId.Value, includeProperties: "CpuPowerConnectors,PcleSlots,M2Slots");

            if (dto.GpuId.HasValue)
                tempBuild.Gpu = await _unitOfWork.Repository<Gpu>().GetFirstOrDefaultAsync(g => g.Id == dto.GpuId.Value, includeProperties: "GpuPowerConnectors");

            if (dto.PowerSupplyId.HasValue)
                tempBuild.PowerSupply = await _unitOfWork.Repository<PowerSupply>().GetFirstOrDefaultAsync(p => p.Id == dto.PowerSupplyId.Value, includeProperties: "PowerSupplyPowerConnectors");

            if (dto.CpuCoolerId.HasValue)
                tempBuild.CpuCooler = await _unitOfWork.Repository<CpuCooler>().GetFirstOrDefaultAsync(c => c.Id == dto.CpuCoolerId.Value, includeProperties: "CpuCoolerSockets");

            if (dto.PcCaseId.HasValue)
                tempBuild.PcCase = await _unitOfWork.Repository<PcCase>().GetFirstOrDefaultAsync(c => c.Id == dto.PcCaseId.Value, includeProperties: "PcCaseFormFactors,PcCaseFanLocations");

            foreach (var ramDto in dto.Rams)
            {
                var ram = await _unitOfWork.Repository<Ram>().GetFirstOrDefaultAsync(r => r.Id == ramDto.ComponentId);
                if (ram != null)
                {
                    tempBuild.PcBuild_Rams.Add(new PcBuild_Ram
                    {
                        Ram = ram,
                        RamId = ram.Id,
                        PcBuild = tempBuild,
                        Quantity = ramDto.Quantity
                    });
                }
            }

            foreach (var ssdDto in dto.Ssds)
            {
                var ssd = await _unitOfWork.Repository<Ssd>().GetFirstOrDefaultAsync(s => s.Id == ssdDto.ComponentId);
                if (ssd != null)
                {
                    tempBuild.PcBuild_Ssds.Add(new PcBuild_Ssd
                    {
                        Ssd = ssd,
                        SsdId = ssd.Id,
                        PcBuild = tempBuild,
                        Quantity = ssdDto.Quantity
                    });
                }
            }

            foreach (var hddDto in dto.Hdds)
            {
                var hdd = await _unitOfWork.Repository<Hdd>().GetFirstOrDefaultAsync(h => h.Id == hddDto.ComponentId);
                if (hdd != null)
                {
                    tempBuild.PcBuild_Hdds.Add(new PcBuild_Hdd
                    {
                        Hdd = hdd,
                        HddId = hdd.Id,
                        PcBuild = tempBuild,
                        Quantity = hddDto.Quantity
                    });
                }
            }

            foreach (var fanDto in dto.Fans)
            {
                var fan = await _unitOfWork.Repository<Fan>().GetFirstOrDefaultAsync(f => f.Id == fanDto.ComponentId);
                if (fan != null)
                {
                    tempBuild.PcBuild_Fans.Add(new PcBuild_Fan
                    {
                        Fan = fan,
                        FanId = fan.Id,
                        PcBuild = tempBuild,
                        Quantity = fanDto.Quantity
                    });
                }
            }

            var results = _compatibilityChecker.CheckAll(tempBuild);
            return results;
        }

        public Task<bool> DeleteBuildAsync(Guid pcBuildId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PcBuild>> GetAllBuildsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PcBuild?> GetBuildByIdAsync(Guid pcBuildId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PcBuild>> GetUserBuildsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveBuildAsync(PcBuild pcBuild)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateBuildAsync(PcBuild pcBuild)
        {
            throw new NotImplementedException();
        }
    }
}
