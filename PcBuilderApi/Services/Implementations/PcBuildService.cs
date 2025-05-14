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

        public async Task<List<CompatibilityResult>> CheckComponentCompatibilityAsync(PcBuild pcBuild, Guid componentId, SD.ComponentType componentType)
        {

            object component = componentType switch
            {
                SD.ComponentType.Cpu => await _unitOfWork.Repository<Cpu>().GetFirstOrDefaultAsync(c => c.Id == componentId),
                SD.ComponentType.Gpu => await _unitOfWork.Repository<Gpu>().GetFirstOrDefaultAsync(m => m.Id == componentId),
                SD.ComponentType.Ram => await _unitOfWork.Repository<Ram>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.Motherboard => await _unitOfWork.Repository<Motherboard>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.CpuCooler => await _unitOfWork.Repository<CpuCooler>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.PcCase => await _unitOfWork.Repository<PcCase>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.PowerSupply => await _unitOfWork.Repository<PowerSupply>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.Ssd => await _unitOfWork.Repository<Ssd>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.Hdd => await _unitOfWork.Repository<Hdd>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                SD.ComponentType.Fan => await _unitOfWork.Repository<Fan>().GetFirstOrDefaultAsync(g => g.Id == componentId),
                _ => throw new InvalidOperationException("Unsupported component type")
            };

            switch (componentType)
            {
                case SD.ComponentType.Cpu:
                    pcBuild.Cpu = (Cpu)component;
                    break;
                case SD.ComponentType.Gpu:
                    pcBuild.Gpu = (Gpu)component;
                    break;
                case SD.ComponentType.Motherboard:
                    pcBuild.Motherboard = (Motherboard)component;
                    break;
                case SD.ComponentType.CpuCooler:
                    pcBuild.CpuCooler = (CpuCooler)component;
                    break;
                case SD.ComponentType.PcCase:
                    pcBuild.PcCase = (PcCase)component;
                    break;
                case SD.ComponentType.PowerSupply:
                    pcBuild.PowerSupply = (PowerSupply)component;
                    break;
                case SD.ComponentType.Ram:
                    {
                        var ram = (Ram)component;
                        var existing = pcBuild.PcBuild_Rams.FirstOrDefault(x => x.RamId == ram.Id);
                        if (existing != null)
                        {
                            existing.Quantity++;
                        }
                        else
                        {
                            pcBuild.PcBuild_Rams.Add(new PcBuild_Ram
                            {
                                RamId = ram.Id,
                                Ram = ram,
                                PcBuildId = pcBuild.Id,
                                PcBuild = pcBuild,
                                Quantity = 1
                            });
                        }
                        break;
                    }
                case SD.ComponentType.Ssd:
                    {
                        var ssd = (Ssd)component;
                        var existing = pcBuild.PcBuild_Ssds.FirstOrDefault(x => x.SsdId == ssd.Id);
                        if (existing != null)
                        {
                            existing.Quantity++;
                        }
                        else
                        {
                            pcBuild.PcBuild_Ssds.Add(new PcBuild_Ssd
                            {
                                SsdId = ssd.Id,
                                Ssd = ssd,
                                PcBuildId = pcBuild.Id,
                                PcBuild = pcBuild,
                                Quantity = 1
                            });
                        }
                        break;
                    }
                case SD.ComponentType.Hdd:
                    {
                        var hdd = (Hdd)component;
                        var existing = pcBuild.PcBuild_Hdds.FirstOrDefault(x => x.HddId == hdd.Id);
                        if (existing != null)
                        {
                            existing.Quantity++;
                        }
                        else
                        {
                            pcBuild.PcBuild_Hdds.Add(new PcBuild_Hdd
                            {
                                HddId = hdd.Id,
                                Hdd = hdd,
                                PcBuildId = pcBuild.Id,
                                PcBuild = pcBuild,
                                Quantity = 1
                            });
                        }
                        break;
                    }
                case SD.ComponentType.Fan:
                    {
                        var fan = (Fan)component;
                        var existing = pcBuild.PcBuild_Fans.FirstOrDefault(x => x.FanId == fan.Id);
                        if (existing != null)
                        {
                            existing.Quantity++;
                        }
                        else
                        {
                            pcBuild.PcBuild_Fans.Add(new PcBuild_Fan
                            {
                                FanId = fan.Id,
                                Fan = fan,
                                PcBuildId = pcBuild.Id,
                                PcBuild = pcBuild,
                                Quantity = 1
                            });
                        }
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unsupported component type");
            }

            var result = _compatibilityChecker.CheckAll(pcBuild);

            return result;
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

        public Task<bool> RemoveComponentFromBuildAsync(PcBuild pcBuild, Guid componentId, SD.ComponentType componentType)
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
