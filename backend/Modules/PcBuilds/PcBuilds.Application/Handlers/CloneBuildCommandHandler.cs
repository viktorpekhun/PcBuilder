using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class CloneBuildCommandHandler : IRequestHandler<CloneBuildCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CloneBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CloneBuildCommand request, CancellationToken cancellationToken)
        {
            var source = await _context.Set<PcBuild>()
                .Include(b => b.PcBuild_Rams)
                .Include(b => b.PcBuild_Ssds)
                .Include(b => b.PcBuild_Hdds)
                .Include(b => b.PcBuild_Fans)
                .FirstOrDefaultAsync(b => b.Id == request.SourceBuildId, cancellationToken);

            if (source == null)
                return Result.Failure<Guid>(new Error("NotFound", "Build not found.", 404));

            if (!source.IsPublished && source.UserId != request.UserId)
                return Result.Failure<Guid>(new Error("Forbidden", "You do not have access to clone this build.", 403));

            var clonedName = source.Name.Length > 245
                ? "Копія " + source.Name[..245]
                : "Копія " + source.Name;

            var clone = new PcBuild
            {
                UserId = request.UserId,
                Name = clonedName,
                Description = source.Description,
                IsPublished = false,
                PublishedAt = null,
                Price = source.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                CpuId = source.CpuId,
                CpuOfferId = source.CpuOfferId,
                GpuId = source.GpuId,
                GpuOfferId = source.GpuOfferId,
                MotherboardId = source.MotherboardId,
                MotherboardOfferId = source.MotherboardOfferId,
                CpuCoolerId = source.CpuCoolerId,
                CpuCoolerOfferId = source.CpuCoolerOfferId,
                PowerSupplyId = source.PowerSupplyId,
                PowerSupplyOfferId = source.PowerSupplyOfferId,
                PcCaseId = source.PcCaseId,
                PcCaseOfferId = source.PcCaseOfferId,
            };

            foreach (var ram in source.PcBuild_Rams)
                clone.PcBuild_Rams.Add(new PcBuild_Ram { RamId = ram.RamId, Quantity = ram.Quantity, ProductOfferId = ram.ProductOfferId });

            foreach (var ssd in source.PcBuild_Ssds)
                clone.PcBuild_Ssds.Add(new PcBuild_Ssd { SsdId = ssd.SsdId, Quantity = ssd.Quantity, ProductOfferId = ssd.ProductOfferId });

            foreach (var hdd in source.PcBuild_Hdds)
                clone.PcBuild_Hdds.Add(new PcBuild_Hdd { HddId = hdd.HddId, Quantity = hdd.Quantity, ProductOfferId = hdd.ProductOfferId });

            foreach (var fan in source.PcBuild_Fans)
                clone.PcBuild_Fans.Add(new PcBuild_Fan { FanId = fan.FanId, Quantity = fan.Quantity, ProductOfferId = fan.ProductOfferId });

            await _context.Set<PcBuild>().AddAsync(clone, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(clone.Id);
        }
    }
}