using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class SaveBuildCommandHandler : IRequestHandler<SaveBuildCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public SaveBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(SaveBuildCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var buildDto = request.BuildDto;

                var pcBuild = new PcBuild
                {
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                pcBuild.Name = buildDto.Name;
                pcBuild.Description = buildDto.Description;
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
                foreach (var ramDto in buildDto.Rams)
                {
                    var ram = await _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == ramDto.ComponentId, cancellationToken);
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

                // Handle SSD components
                foreach (var ssdDto in buildDto.Ssds)
                {
                    var ssd = await _context.Set<Ssd>().FirstOrDefaultAsync(s => s.Id == ssdDto.ComponentId, cancellationToken);
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

                // Handle HDD components
                foreach (var hddDto in buildDto.Hdds)
                {
                    var hdd = await _context.Set<Hdd>().FirstOrDefaultAsync(h => h.Id == hddDto.ComponentId, cancellationToken);
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

                // Handle Fan components
                foreach (var fanDto in buildDto.Fans)
                {
                    var fan = await _context.Set<Fan>().FirstOrDefaultAsync(f => f.Id == fanDto.ComponentId, cancellationToken);
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

                await UpdateTotalPrice(pcBuild, cancellationToken);

                await _context.Set<PcBuild>().AddAsync(pcBuild, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
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
