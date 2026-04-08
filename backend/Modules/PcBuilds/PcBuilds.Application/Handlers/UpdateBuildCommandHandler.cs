using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class UpdateBuildCommandHandler : IRequestHandler<UpdateBuildCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateBuildCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var buildDto = request.BuildDto;

                var pcBuild = await _context.Set<PcBuild>()
                    .Include(b => b.PcBuild_Rams)
                    .Include(b => b.PcBuild_Ssds)
                    .Include(b => b.PcBuild_Hdds)
                    .Include(b => b.PcBuild_Fans)
                    .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

                if (pcBuild == null)
                    return false;

                pcBuild.Name = buildDto.Name;
                pcBuild.Description = buildDto.Description;

                if (buildDto.IsPublished && !pcBuild.IsPublished)
                    pcBuild.PublishedAt = DateTime.UtcNow;
                else if (!buildDto.IsPublished)
                    pcBuild.PublishedAt = null;

                pcBuild.IsPublished = buildDto.IsPublished;
                pcBuild.UpdatedAt = DateTime.UtcNow;

                pcBuild.CpuId = buildDto.CpuId;
                pcBuild.GpuId = buildDto.GpuId;
                pcBuild.MotherboardId = buildDto.MotherboardId;
                pcBuild.CpuCoolerId = buildDto.CpuCoolerId;
                pcBuild.PowerSupplyId = buildDto.PowerSupplyId;
                pcBuild.PcCaseId = buildDto.PcCaseId;

                pcBuild.CpuOfferId = buildDto.CpuOfferId;
                pcBuild.GpuOfferId = buildDto.GpuOfferId;
                pcBuild.MotherboardOfferId = buildDto.MotherboardOfferId;
                pcBuild.CpuCoolerOfferId = buildDto.CpuCoolerOfferId;
                pcBuild.PowerSupplyOfferId = buildDto.PowerSupplyOfferId;
                pcBuild.PcCaseOfferId = buildDto.PcCaseOfferId;

                // Handle RAM components
                UpdateMultiComponents(
                    pcBuild.PcBuild_Rams,
                    buildDto.Rams,
                    r => r.RamId,
                    async (dto) =>
                    {
                        var ram = await _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == dto.ComponentId, cancellationToken);
                        return ram == null ? null : new PcBuild_Ram
                        {
                            RamId = ram.Id,
                            PcBuild = pcBuild,
                            PcBuildId = pcBuild.Id,
                            Quantity = dto.Quantity,
                            ProductOfferId = dto.OfferId
                        };
                    },
                    (existing, dto) =>
                    {
                        existing.Quantity = dto.Quantity;
                        existing.ProductOfferId = dto.OfferId;
                    });

                // Handle SSD components
                UpdateMultiComponents(
                    pcBuild.PcBuild_Ssds,
                    buildDto.Ssds,
                    s => s.SsdId,
                    async (dto) =>
                    {
                        var ssd = await _context.Set<Ssd>().FirstOrDefaultAsync(s => s.Id == dto.ComponentId, cancellationToken);
                        return ssd == null ? null : new PcBuild_Ssd
                        {
                            SsdId = ssd.Id,
                            PcBuild = pcBuild,
                            PcBuildId = pcBuild.Id,
                            Quantity = dto.Quantity,
                            ProductOfferId = dto.OfferId
                        };
                    },
                    (existing, dto) =>
                    {
                        existing.Quantity = dto.Quantity;
                        existing.ProductOfferId = dto.OfferId;
                    });

                // Handle HDD components
                UpdateMultiComponents(
                    pcBuild.PcBuild_Hdds,
                    buildDto.Hdds,
                    h => h.HddId,
                    async (dto) =>
                    {
                        var hdd = await _context.Set<Hdd>().FirstOrDefaultAsync(h => h.Id == dto.ComponentId, cancellationToken);
                        return hdd == null ? null : new PcBuild_Hdd
                        {
                            HddId = hdd.Id,
                            PcBuild = pcBuild,
                            PcBuildId = pcBuild.Id,
                            Quantity = dto.Quantity,
                            ProductOfferId = dto.OfferId
                        };
                    },
                    (existing, dto) =>
                    {
                        existing.Quantity = dto.Quantity;
                        existing.ProductOfferId = dto.OfferId;
                    });

                // Handle Fan components
                UpdateMultiComponents(
                    pcBuild.PcBuild_Fans,
                    buildDto.Fans,
                    f => f.FanId,
                    async (dto) =>
                    {
                        var fan = await _context.Set<Fan>().FirstOrDefaultAsync(f => f.Id == dto.ComponentId, cancellationToken);
                        return fan == null ? null : new PcBuild_Fan
                        {
                            FanId = fan.Id,
                            PcBuild = pcBuild,
                            PcBuildId = pcBuild.Id,
                            Quantity = dto.Quantity,
                            ProductOfferId = dto.OfferId
                        };
                    },
                    (existing, dto) =>
                    {
                        existing.Quantity = dto.Quantity;
                        existing.ProductOfferId = dto.OfferId;
                    });

                await UpdateTotalPrice(pcBuild, cancellationToken);
                _context.Set<PcBuild>().Update(pcBuild);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateMultiComponents<T>(
            ICollection<T> existingItems,
            List<Dtos.ComponentQuantityDto> dtoItems,
            Func<T, Guid> getComponentId,
            Func<Dtos.ComponentQuantityDto, Task<T?>> createNew,
            Action<T, Dtos.ComponentQuantityDto> updateExisting)
        {
            var existingDict = existingItems.ToDictionary(getComponentId);

            foreach (var dto in dtoItems)
            {
                if (existingDict.TryGetValue(dto.ComponentId, out var existing))
                {
                    updateExisting(existing, dto);
                    existingDict.Remove(dto.ComponentId);
                }
                else
                {
                    var newItem = createNew(dto).GetAwaiter().GetResult();
                    if (newItem != null)
                        existingItems.Add(newItem);
                }
            }

            foreach (var toRemove in existingDict.Values)
            {
                existingItems.Remove(toRemove);
            }
        }

        private async Task UpdateTotalPrice(PcBuild pcBuild, CancellationToken cancellationToken)
        {
            decimal totalPrice = 0;

            totalPrice += await GetComponentPrice(pcBuild.CpuOfferId, cancellationToken);
            totalPrice += await GetComponentPrice(pcBuild.GpuOfferId, cancellationToken);
            totalPrice += await GetComponentPrice(pcBuild.MotherboardOfferId, cancellationToken);
            totalPrice += await GetComponentPrice(pcBuild.CpuCoolerOfferId, cancellationToken);
            totalPrice += await GetComponentPrice(pcBuild.PowerSupplyOfferId, cancellationToken);
            totalPrice += await GetComponentPrice(pcBuild.PcCaseOfferId, cancellationToken);

            foreach (var ram in pcBuild.PcBuild_Rams)
            {
                decimal ramPrice = await GetComponentPrice(ram.ProductOfferId, cancellationToken);
                totalPrice += ramPrice * ram.Quantity;
            }

            foreach (var ssd in pcBuild.PcBuild_Ssds)
            {
                decimal ssdPrice = await GetComponentPrice(ssd.ProductOfferId, cancellationToken);
                totalPrice += ssdPrice * ssd.Quantity;
            }

            foreach (var hdd in pcBuild.PcBuild_Hdds)
            {
                decimal hddPrice = await GetComponentPrice(hdd.ProductOfferId, cancellationToken);
                totalPrice += hddPrice * hdd.Quantity;
            }

            foreach (var fan in pcBuild.PcBuild_Fans)
            {
                decimal fanPrice = await GetComponentPrice(fan.ProductOfferId, cancellationToken);
                totalPrice += fanPrice * fan.Quantity;
            }

            pcBuild.Price = totalPrice;
        }

        private async Task<decimal> GetComponentPrice(Guid? offerId, CancellationToken cancellationToken)
        {
            if (!offerId.HasValue)
                return 0;

            var offer = await _context.Set<ProductOffer>().FirstOrDefaultAsync(o => o.Id == offerId, cancellationToken);
            return offer?.Price ?? 0;
        }
    }
}
