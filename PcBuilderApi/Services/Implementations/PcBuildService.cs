using PcBuilderApi.Dtos.PcBuildDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Compatibility;
using PcBuilderApi.Services.Interfaces;


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

        public async Task<bool> DeleteBuildAsync(Guid pcBuildId, Guid userId)
        {
            try
            {
                var build = await _unitOfWork.Repository<PcBuild>().GetFirstOrDefaultAsync(b => b.Id == pcBuildId);
                if (build == null)
                {
                    return false;
                }
                if (build.UserId != userId)
                {
                    return false;
                }
                await _unitOfWork.Repository<PcBuild>().DeleteAsync(build);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<List<PcBuild>> GetAllBuildsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<PcBuildRequestDto?> GetBuildByIdAsync(Guid pcBuildId)
        {
            try
            {
                var build = await _unitOfWork.Repository<PcBuild>()
                        .GetFirstOrDefaultAsync(
                            filter: b => b.Id == pcBuildId,
                            includeProperties: "Cpu,Gpu,Motherboard,CpuCooler,PcCase,PowerSupply,PcBuild_Rams,PcBuild_Ssds,PcBuild_Hdds,PcBuild_Fans"
                        );

                if (build == null)
                {
                    return null;
                }

                
                var cpuInfo = await GetComponentPriceAndStore(build.CpuId, build.CpuOfferId);
                var gpuInfo = await GetComponentPriceAndStore(build.GpuId, build.GpuOfferId);
                var motherboardInfo = await GetComponentPriceAndStore(build.MotherboardId, build.MotherboardOfferId);
                var cpuCoolerInfo = await GetComponentPriceAndStore(build.CpuCoolerId, build.CpuCoolerOfferId);
                var powerSupplyInfo = await GetComponentPriceAndStore(build.PowerSupplyId, build.PowerSupplyOfferId);
                var pcCaseInfo = await GetComponentPriceAndStore(build.PcCaseId, build.PcCaseOfferId);
                var buildDto = new PcBuildRequestDto
                {
                    Id = build.Id,
                    Name = build.Name,
                    Description = build.Description,
                    IsPublished = build.IsPublished,
                    Price = build.Price,
                    CreatedAt = build.CreatedAt,
                    UpdatedAt = build.UpdatedAt,
                    UserId = build.UserId,

                    // Count all components
                    
                    Cpu = (build.Cpu != null && build.CpuOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.Cpu.Id,
                        Name = build.Cpu.Name,
                        Price = cpuInfo.Item1,
                        OfferId = (Guid)build.CpuOfferId,
                        StoreName = cpuInfo.Item2,
                        ImageUrl = build.Cpu.PhotoUrl
                    } : null,

                    // Get GPU preview if exists
                    Gpu = (build.Gpu != null && build.GpuOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.Gpu.Id,
                        Name = build.Gpu.Name,
                        Price = gpuInfo.Item1,
                        OfferId = (Guid)build.GpuOfferId,
                        StoreName = gpuInfo.Item2,
                        ImageUrl = build.Gpu.PhotoUrl
                    } : null,

                    Motherboard = (build.Motherboard != null && build.MotherboardOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.Motherboard.Id,
                        Name = build.Motherboard.Name,
                        Price = motherboardInfo.Item1,
                        OfferId = (Guid)build.MotherboardOfferId,
                        StoreName = motherboardInfo.Item2,
                        ImageUrl = build.Motherboard.PhotoUrl
                    } : null,
                    CpuCooler = (build.CpuCooler != null && build.CpuCoolerOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.CpuCooler.Id,
                        Name = build.CpuCooler.Name,
                        Price = cpuCoolerInfo.Item1,
                        StoreName = cpuCoolerInfo.Item2,
                        ImageUrl = build.CpuCooler.PhotoUrl
                    } : null,
                    PowerSupply = (build.PowerSupply != null && build.PowerSupplyOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.PowerSupply.Id,
                        Name = build.PowerSupply.Name,
                        Price = powerSupplyInfo.Item1,
                        OfferId = (Guid)build.PowerSupplyOfferId,
                        StoreName = powerSupplyInfo.Item2,
                        ImageUrl = build.PowerSupply.PhotoUrl
                    } : null,
                    PcCase = (build.PcCase != null && build.PcCaseOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.PcCase.Id,
                        Name = build.PcCase.Name,
                        Price = pcCaseInfo.Item1,
                        OfferId = (Guid)build.PcCaseOfferId,
                        StoreName = pcCaseInfo.Item2,
                        ImageUrl = build.PcCase.PhotoUrl
                    } : null

                };
                buildDto.Rams = new List<MultiComponentPreviewDto>();
                if (build.PcBuild_Rams != null && build.PcBuild_Rams.Any())
                {
                    foreach (var pcBuildRam in build.PcBuild_Rams)
                    {
                        // If navigation property is loaded, use it directly
                        if(pcBuildRam.ProductOfferId == null) continue;
                        Ram ram = pcBuildRam.Ram;
                        if (ram == null)
                        {
                            // Otherwise fetch from database
                            ram = await _unitOfWork.Repository<Ram>().GetFirstOrDefaultAsync(r => r.Id == pcBuildRam.RamId);
                            if (ram == null) continue;
                        }
                        
                        // Get price
                        var ramInfo = await GetComponentPriceAndStore(pcBuildRam.RamId, pcBuildRam.ProductOfferId);

                        buildDto.Rams.Add(new MultiComponentPreviewDto
                        {
                            Id = pcBuildRam.RamId,
                            Name = ram.Name,
                            Quantity = pcBuildRam.Quantity,
                            StoreName = ramInfo.Item2,
                            OfferId = (Guid)pcBuildRam.ProductOfferId,
                            TotalPrice = ramInfo.Item1 * pcBuildRam.Quantity,
                            ImageUrl = ram.PhotoUrl
                        });
                    }
                }

                buildDto.Ssds = new List<MultiComponentPreviewDto>();
                if (build.PcBuild_Ssds != null && build.PcBuild_Ssds.Any())
                {
                    foreach (var pcBuildSsd in build.PcBuild_Ssds)
                    {
                        // If navigation property is loaded, use it directly
                        if (pcBuildSsd.ProductOfferId == null) continue;
                        Ssd ssd = pcBuildSsd.Ssd;
                        if (ssd == null)
                        {
                            // Otherwise fetch from database
                            ssd = await _unitOfWork.Repository<Ssd>().GetFirstOrDefaultAsync(s => s.Id == pcBuildSsd.SsdId);
                            if (ssd == null) continue;
                        }

                        // Get price
                        var ssdInfo = await GetComponentPriceAndStore(pcBuildSsd.SsdId, pcBuildSsd.ProductOfferId);

                        buildDto.Ssds.Add(new MultiComponentPreviewDto
                        {
                            Id = pcBuildSsd.SsdId,
                            Name = ssd.Name,
                            Quantity = pcBuildSsd.Quantity,
                            StoreName = ssdInfo.Item2,
                            OfferId = (Guid)pcBuildSsd.ProductOfferId,
                            TotalPrice = ssdInfo.Item1 * pcBuildSsd.Quantity,
                            ImageUrl = ssd.PhotoUrl
                        });
                    }
                }

                buildDto.Hdds = new List<MultiComponentPreviewDto>();
                if (build.PcBuild_Hdds != null && build.PcBuild_Hdds.Any())
                {
                    foreach (var pcBuildHdd in build.PcBuild_Hdds)
                    {
                        // If navigation property is loaded, use it directly
                        if (pcBuildHdd.ProductOfferId == null) continue;
                        Hdd hdd = pcBuildHdd.Hdd;
                        if (hdd == null)
                        {
                            // Otherwise fetch from database
                            hdd = await _unitOfWork.Repository<Hdd>().GetFirstOrDefaultAsync(h => h.Id == pcBuildHdd.HddId);
                            if (hdd == null) continue;
                        }

                        // Get price
                        var hddInfo = await GetComponentPriceAndStore(pcBuildHdd.HddId, pcBuildHdd.ProductOfferId);

                        buildDto.Hdds.Add(new MultiComponentPreviewDto
                        {
                            Id = pcBuildHdd.HddId,
                            Name = hdd.Name,
                            Quantity = pcBuildHdd.Quantity,
                            StoreName = hddInfo.Item2,
                            OfferId = (Guid)pcBuildHdd.ProductOfferId,
                            TotalPrice = hddInfo.Item1 * pcBuildHdd.Quantity,
                            ImageUrl = hdd.PhotoUrl
                        });
                    }
                }

                buildDto.Fans = new List<MultiComponentPreviewDto>();
                if (build.PcBuild_Fans != null && build.PcBuild_Fans.Any())
                {
                    foreach (var pcBuildFan in build.PcBuild_Fans)
                    {
                        // If navigation property is loaded, use it directly
                        if (pcBuildFan.ProductOfferId == null) continue;
                        Fan fan = pcBuildFan.Fan;
                        if (fan == null)
                        {
                            // Otherwise fetch from database
                            fan = await _unitOfWork.Repository<Fan>().GetFirstOrDefaultAsync(f => f.Id == pcBuildFan.FanId);
                            if (fan == null) continue;
                        }

                        // Get price
                        var fanInfo = await GetComponentPriceAndStore(pcBuildFan.FanId, pcBuildFan.ProductOfferId);

                        buildDto.Fans.Add(new MultiComponentPreviewDto
                        {
                            Id = pcBuildFan.FanId,
                            Name = fan.Name,
                            Quantity = pcBuildFan.Quantity,
                            StoreName = fanInfo.Item2,
                            OfferId = (Guid)pcBuildFan.ProductOfferId,
                            TotalPrice = fanInfo.Item1 * pcBuildFan.Quantity,
                            ImageUrl = fan.PhotoUrl
                        });
                    }
                }

                return buildDto;
            }
            catch (Exception ex)
            {
                return new PcBuildRequestDto();
            }
        }

        public async Task<List<PcBuildListDto>> GetUserBuildsAsync(Guid userId)
        {
            try
            {
                // Fetch user's builds with minimal includes for performance
                var builds = await _unitOfWork.Repository<PcBuild>()
                    .GetAllAsync(
                        filter: b => b.UserId == userId
                    );

                var buildDtos = new List<PcBuildListDto>();

                foreach (var build in builds)
                {
                    var buildDto = new PcBuildListDto
                    {
                        Id = build.Id,
                        Name = build.Name,
                        Price = build.Price,
                        UpdatedAt = build.UpdatedAt                        
                    };
                    
                    buildDtos.Add(buildDto);
                }

                return buildDtos;
            }
            catch (Exception ex)
            {
                return new List<PcBuildListDto>();
            }
        }

        public async Task<bool> SaveBuildAsync(PcBuildInputDto buildDto, Guid userId)
        {
            try
            {
                PcBuild pcBuild;
                bool isNewBuild = !buildDto.Id.HasValue;

                // If updating existing build, retrieve it and verify ownership
                if (!isNewBuild)
                {
                    pcBuild = await _unitOfWork.Repository<PcBuild>()
                        .GetFirstOrDefaultAsync(
                            b => b.Id == buildDto.Id.Value,
                            includeProperties: "PcBuild_Rams,PcBuild_Ssds,PcBuild_Hdds,PcBuild_Fans"
                        );

                    if (pcBuild == null)
                    {
                        return false;
                    }

                    if (pcBuild.UserId != userId)
                    {
                        return false;
                    }
                }
                else
                {
                    // Create new build
                    pcBuild = new PcBuild
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Update basic properties
                pcBuild.Name = buildDto.Name;
                pcBuild.Description = buildDto.Description;
                pcBuild.IsPublished = buildDto.IsPublished;
                pcBuild.UpdatedAt = DateTime.UtcNow;

                // Update main components
                pcBuild.CpuId = buildDto.CpuId;
                pcBuild.GpuId = buildDto.GpuId;
                pcBuild.MotherboardId = buildDto.MotherboardId;
                pcBuild.CpuCoolerId = buildDto.CpuCoolerId;
                pcBuild.PowerSupplyId = buildDto.PowerSupplyId;
                pcBuild.PcCaseId = buildDto.PcCaseId;

                // Update component offers
                pcBuild.CpuOfferId = buildDto.CpuOfferId;
                pcBuild.GpuOfferId = buildDto.GpuOfferId;
                pcBuild.MotherboardOfferId = buildDto.MotherboardOfferId;
                pcBuild.CpuCoolerOfferId = buildDto.CpuCoolerOfferId;
                pcBuild.PowerSupplyOfferId = buildDto.PowerSupplyOfferId;
                pcBuild.PcCaseOfferId = buildDto.PcCaseOfferId;

                // Handle RAM components
                var existingRams = pcBuild.PcBuild_Rams.ToDictionary(r => r.RamId, r => r);

                // 2. Process each new RAM component
                foreach (var ramDto in buildDto.Rams)
                {
                    if (existingRams.TryGetValue(ramDto.ComponentId, out var existingRam))
                    {
                        // Update existing RAM
                        existingRam.Quantity = ramDto.Quantity;
                        existingRam.ProductOfferId = ramDto.OfferId;
                        existingRams.Remove(ramDto.ComponentId); // Remove from dictionary to track processed items
                    }
                    else
                    {
                        // Add new RAM
                        var ram = await _unitOfWork.Repository<Ram>().GetFirstOrDefaultAsync(r => r.Id == ramDto.ComponentId);
                        if (ram != null)
                        {
                            pcBuild.PcBuild_Rams.Add(new PcBuild_Ram
                            {
                                RamId = ram.Id,
                                PcBuild = pcBuild,
                                PcBuildId = pcBuild.Id,
                                Quantity = ramDto.Quantity,
                                ProductOfferId = ramDto.OfferId
                            });
                        }
                    }
                }

                // 3. Remove RAMs that are no longer in the build
                foreach (var ramToRemove in existingRams.Values)
                {
                    pcBuild.PcBuild_Rams.Remove(ramToRemove);
                }

                // Handle SSD components
                // 1. Create dictionary of existing SSD components by ID
                var existingSsds = pcBuild.PcBuild_Ssds.ToDictionary(s => s.SsdId, s => s);

                // 2. Process each new SSD component
                foreach (var ssdDto in buildDto.Ssds)
                {
                    if (existingSsds.TryGetValue(ssdDto.ComponentId, out var existingSsd))
                    {
                        // Update existing SSD
                        existingSsd.Quantity = ssdDto.Quantity;
                        existingSsd.ProductOfferId = ssdDto.OfferId;
                        existingSsds.Remove(ssdDto.ComponentId);
                    }
                    else
                    {
                        // Add new SSD
                        var ssd = await _unitOfWork.Repository<Ssd>().GetFirstOrDefaultAsync(s => s.Id == ssdDto.ComponentId);
                        if (ssd != null)
                        {
                            pcBuild.PcBuild_Ssds.Add(new PcBuild_Ssd
                            {
                                SsdId = ssd.Id,
                                PcBuild = pcBuild,
                                PcBuildId = pcBuild.Id,
                                Quantity = ssdDto.Quantity,
                                ProductOfferId = ssdDto.OfferId
                            });
                        }
                    }
                }

                // 3. Remove SSDs that are no longer in the build
                foreach (var ssdToRemove in existingSsds.Values)
                {
                    pcBuild.PcBuild_Ssds.Remove(ssdToRemove);
                }

                // Handle HDD components
                // 1. Create dictionary of existing HDD components by ID
                var existingHdds = pcBuild.PcBuild_Hdds.ToDictionary(h => h.HddId, h => h);

                // 2. Process each new HDD component
                foreach (var hddDto in buildDto.Hdds)
                {
                    if (existingHdds.TryGetValue(hddDto.ComponentId, out var existingHdd))
                    {
                        // Update existing HDD
                        existingHdd.Quantity = hddDto.Quantity;
                        existingHdd.ProductOfferId = hddDto.OfferId;
                        existingHdds.Remove(hddDto.ComponentId);
                    }
                    else
                    {
                        // Add new HDD
                        var hdd = await _unitOfWork.Repository<Hdd>().GetFirstOrDefaultAsync(h => h.Id == hddDto.ComponentId);
                        if (hdd != null)
                        {
                            pcBuild.PcBuild_Hdds.Add(new PcBuild_Hdd
                            {
                                HddId = hdd.Id,
                                PcBuild = pcBuild,
                                PcBuildId = pcBuild.Id,
                                Quantity = hddDto.Quantity,
                                ProductOfferId = hddDto.OfferId
                            });
                        }
                    }
                }

                // 3. Remove HDDs that are no longer in the build
                foreach (var hddToRemove in existingHdds.Values)
                {
                    pcBuild.PcBuild_Hdds.Remove(hddToRemove);
                }

                // Handle Fan components
                // 1. Create dictionary of existing Fan components by ID
                var existingFans = pcBuild.PcBuild_Fans.ToDictionary(f => f.FanId, f => f);

                // 2. Process each new Fan component
                foreach (var fanDto in buildDto.Fans)
                {
                    if (existingFans.TryGetValue(fanDto.ComponentId, out var existingFan))
                    {
                        // Update existing Fan
                        existingFan.Quantity = fanDto.Quantity;
                        existingFan.ProductOfferId = fanDto.OfferId;
                        existingFans.Remove(fanDto.ComponentId);
                    }
                    else
                    {
                        // Add new Fan
                        var fan = await _unitOfWork.Repository<Fan>().GetFirstOrDefaultAsync(f => f.Id == fanDto.ComponentId);
                        if (fan != null)
                        {
                            pcBuild.PcBuild_Fans.Add(new PcBuild_Fan
                            {
                                FanId = fan.Id,
                                PcBuild = pcBuild,
                                PcBuildId = pcBuild.Id,
                                Quantity = fanDto.Quantity,
                                ProductOfferId = fanDto.OfferId
                            });
                        }
                    }
                }

                // 3. Remove Fans that are no longer in the build
                foreach (var fanToRemove in existingFans.Values)
                {
                    pcBuild.PcBuild_Fans.Remove(fanToRemove);
                }

                // Calculate total price
                await UpdateTotalPrice(pcBuild);

                // Save to database
                if (isNewBuild)
                {
                    await _unitOfWork.Repository<PcBuild>().AddAsync(pcBuild);
                }
                else
                {
                    await _unitOfWork.Repository<PcBuild>().UpdateAsync(pcBuild);
                }

                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                return false;
            }
        }

        public Task<bool> UpdateBuildAsync(PcBuild pcBuild)
        {
            throw new NotImplementedException();
        }

        private async Task UpdateTotalPrice(PcBuild pcBuild)
        {
            decimal totalPrice = 0;

            // Calculate price for individual components
            totalPrice += await GetComponentPrice(pcBuild.CpuId, pcBuild.CpuOfferId);
            totalPrice += await GetComponentPrice(pcBuild.GpuId, pcBuild.GpuOfferId);
            totalPrice += await GetComponentPrice(pcBuild.MotherboardId, pcBuild.MotherboardOfferId);
            totalPrice += await GetComponentPrice(pcBuild.CpuCoolerId, pcBuild.CpuCoolerOfferId);
            totalPrice += await GetComponentPrice(pcBuild.PowerSupplyId, pcBuild.PowerSupplyOfferId);
            totalPrice += await GetComponentPrice(pcBuild.PcCaseId, pcBuild.PcCaseOfferId);

            // Calculate price for RAM components
            foreach (var ram in pcBuild.PcBuild_Rams)
            {
                decimal ramPrice = await GetComponentPrice(ram.RamId, ram.ProductOfferId);
                totalPrice += ramPrice * ram.Quantity;
            }

            // Calculate price for SSD components
            foreach (var ssd in pcBuild.PcBuild_Ssds)
            {
                decimal ssdPrice = await GetComponentPrice(ssd.SsdId, ssd.ProductOfferId);
                totalPrice += ssdPrice * ssd.Quantity;
            }

            // Calculate price for HDD components
            foreach (var hdd in pcBuild.PcBuild_Hdds)
            {
                decimal hddPrice = await GetComponentPrice(hdd.HddId, hdd.ProductOfferId);
                totalPrice += hddPrice * hdd.Quantity;
            }

            // Calculate price for Fan components
            foreach (var fan in pcBuild.PcBuild_Fans)
            {
                decimal fanPrice = await GetComponentPrice(fan.FanId, fan.ProductOfferId);
                totalPrice += fanPrice * fan.Quantity;
            }

            pcBuild.Price = totalPrice;
        }

        private async Task<(decimal, string)> GetComponentPriceAndStore(Guid? componentId, Guid? offerId)
        {
            if (!componentId.HasValue)
            {
                return (0, string.Empty);
            }


            var offer = await _unitOfWork.Repository<ProductOffer>().GetFirstOrDefaultAsync(filter: o => o.Id == offerId,
                includeProperties: "Store");
            if (offer != null)
            {
                return (offer.Price, offer.Store.Name);
            }
            else
            {
                return (0, string.Empty);
            }


        }
        private async Task<decimal> GetComponentPrice(Guid? componentId, Guid? offerId)
        {
            if (!componentId.HasValue)
            {
                return 0;
            }

            var offer = await _unitOfWork.Repository<ProductOffer>().GetFirstOrDefaultAsync(filter: o => o.Id == offerId);
            if (offer != null)
            {
                return offer.Price;
            }
            else
            {
                return 0;
            }
        }
    }
}
