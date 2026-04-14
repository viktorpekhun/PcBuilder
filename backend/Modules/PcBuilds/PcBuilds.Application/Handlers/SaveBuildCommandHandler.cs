using Auth.Domain.Entities;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class SaveBuildCommandHandler : IRequestHandler<SaveBuildCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public SaveBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(SaveBuildCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user != null && user.PostBanUntil > DateTime.UtcNow)
                return Result.Failure<bool>(new Error("Forbidden", "You are temporarily banned from posting.", 403));

            var buildDto = request.BuildDto;

            var pcBuild = new PcBuild
            {
                UserId = request.UserId,
                Name = buildDto.Name,
                Description = buildDto.Description,
                IsPublished = buildDto.IsPublished,
                PublishedAt = buildDto.IsPublished ? DateTime.UtcNow : null,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CpuId = buildDto.CpuId,
                GpuId = buildDto.GpuId,
                MotherboardId = buildDto.MotherboardId,
                CpuCoolerId = buildDto.CpuCoolerId,
                PowerSupplyId = buildDto.PowerSupplyId,
                PcCaseId = buildDto.PcCaseId,
                CpuOfferId = buildDto.CpuOfferId,
                GpuOfferId = buildDto.GpuOfferId,
                MotherboardOfferId = buildDto.MotherboardOfferId,
                CpuCoolerOfferId = buildDto.CpuCoolerOfferId,
                PowerSupplyOfferId = buildDto.PowerSupplyOfferId,
                PcCaseOfferId = buildDto.PcCaseOfferId,
            };

            foreach (var ramDto in buildDto.Rams)
            {
                var ram = await _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == ramDto.ComponentId, cancellationToken);
                if (ram != null)
                    pcBuild.PcBuild_Rams.Add(new PcBuild_Ram { RamId = ram.Id, PcBuild = pcBuild, PcBuildId = pcBuild.Id, Quantity = ramDto.Quantity, ProductOfferId = ramDto.OfferId });
            }

            foreach (var ssdDto in buildDto.Ssds)
            {
                var ssd = await _context.Set<Ssd>().FirstOrDefaultAsync(s => s.Id == ssdDto.ComponentId, cancellationToken);
                if (ssd != null)
                    pcBuild.PcBuild_Ssds.Add(new PcBuild_Ssd { SsdId = ssd.Id, PcBuild = pcBuild, PcBuildId = pcBuild.Id, Quantity = ssdDto.Quantity, ProductOfferId = ssdDto.OfferId });
            }

            foreach (var hddDto in buildDto.Hdds)
            {
                var hdd = await _context.Set<Hdd>().FirstOrDefaultAsync(h => h.Id == hddDto.ComponentId, cancellationToken);
                if (hdd != null)
                    pcBuild.PcBuild_Hdds.Add(new PcBuild_Hdd { HddId = hdd.Id, PcBuild = pcBuild, PcBuildId = pcBuild.Id, Quantity = hddDto.Quantity, ProductOfferId = hddDto.OfferId });
            }

            foreach (var fanDto in buildDto.Fans)
            {
                var fan = await _context.Set<Fan>().FirstOrDefaultAsync(f => f.Id == fanDto.ComponentId, cancellationToken);
                if (fan != null)
                    pcBuild.PcBuild_Fans.Add(new PcBuild_Fan { FanId = fan.Id, PcBuild = pcBuild, PcBuildId = pcBuild.Id, Quantity = fanDto.Quantity, ProductOfferId = fanDto.OfferId });
            }

            await UpdateTotalPrice(pcBuild, cancellationToken);

            await _context.Set<PcBuild>().AddAsync(pcBuild, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
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
                totalPrice += await GetComponentPrice(ram.ProductOfferId, cancellationToken) * ram.Quantity;

            foreach (var ssd in pcBuild.PcBuild_Ssds)
                totalPrice += await GetComponentPrice(ssd.ProductOfferId, cancellationToken) * ssd.Quantity;

            foreach (var hdd in pcBuild.PcBuild_Hdds)
                totalPrice += await GetComponentPrice(hdd.ProductOfferId, cancellationToken) * hdd.Quantity;

            foreach (var fan in pcBuild.PcBuild_Fans)
                totalPrice += await GetComponentPrice(fan.ProductOfferId, cancellationToken) * fan.Quantity;

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