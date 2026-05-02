using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Compatibility;
using PcBuilds.Application.Queries;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class CheckCompatibilityQueryHandler : IRequestHandler<CheckCompatibilityQuery, Result<BuildCompatibilityReport>>
    {
        private readonly IApplicationDbContext _context;
        private readonly CompatibilityChecker _compatibilityChecker;

        public CheckCompatibilityQueryHandler(IApplicationDbContext context, CompatibilityChecker compatibilityChecker)
        {
            _context = context;
            _compatibilityChecker = compatibilityChecker;
        }

        public async Task<Result<BuildCompatibilityReport>> Handle(CheckCompatibilityQuery request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var tempBuild = new PcBuild();

            if (dto.CpuId.HasValue)
                tempBuild.Cpu = await _context.Set<Cpu>().FirstOrDefaultAsync(c => c.Id == dto.CpuId.Value, cancellationToken);

            if (dto.MotherboardId.HasValue)
                tempBuild.Motherboard = await _context.Set<Motherboard>()
                    .Include(m => m.CpuPowerConnectors).Include(m => m.PcleSlots).Include(m => m.M2Slots)
                    .FirstOrDefaultAsync(m => m.Id == dto.MotherboardId.Value, cancellationToken);

            if (dto.GpuId.HasValue)
                tempBuild.Gpu = await _context.Set<Gpu>()
                    .Include(g => g.GpuPowerConnectors)
                    .FirstOrDefaultAsync(g => g.Id == dto.GpuId.Value, cancellationToken);

            if (dto.PowerSupplyId.HasValue)
                tempBuild.PowerSupply = await _context.Set<PowerSupply>()
                    .Include(p => p.PowerSupplyPowerConnectors)
                    .FirstOrDefaultAsync(p => p.Id == dto.PowerSupplyId.Value, cancellationToken);

            if (dto.CpuCoolerId.HasValue)
                tempBuild.CpuCooler = await _context.Set<CpuCooler>()
                    .Include(c => c.CpuCoolerSockets)
                    .FirstOrDefaultAsync(c => c.Id == dto.CpuCoolerId.Value, cancellationToken);

            if (dto.PcCaseId.HasValue)
                tempBuild.PcCase = await _context.Set<PcCase>()
                    .Include(c => c.PcCaseFormFactors).Include(c => c.PcCaseFanLocations)
                    .FirstOrDefaultAsync(c => c.Id == dto.PcCaseId.Value, cancellationToken);

            foreach (var ramDto in dto.Rams)
            {
                var ram = await _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == ramDto.ComponentId, cancellationToken);
                if (ram != null)
                {
                    tempBuild.PcBuild_Rams.Add(new PcBuild_Ram
                    {
                        Ram = ram, RamId = ram.Id, PcBuild = tempBuild, Quantity = ramDto.Quantity
                    });
                }
            }

            foreach (var ssdDto in dto.Ssds)
            {
                var ssd = await _context.Set<Ssd>().FirstOrDefaultAsync(s => s.Id == ssdDto.ComponentId, cancellationToken);
                if (ssd != null)
                {
                    tempBuild.PcBuild_Ssds.Add(new PcBuild_Ssd
                    {
                        Ssd = ssd, SsdId = ssd.Id, PcBuild = tempBuild, Quantity = ssdDto.Quantity
                    });
                }
            }

            foreach (var hddDto in dto.Hdds)
            {
                var hdd = await _context.Set<Hdd>().FirstOrDefaultAsync(h => h.Id == hddDto.ComponentId, cancellationToken);
                if (hdd != null)
                {
                    tempBuild.PcBuild_Hdds.Add(new PcBuild_Hdd
                    {
                        Hdd = hdd, HddId = hdd.Id, PcBuild = tempBuild, Quantity = hddDto.Quantity
                    });
                }
            }

            foreach (var fanDto in dto.Fans)
            {
                var fan = await _context.Set<Fan>().FirstOrDefaultAsync(f => f.Id == fanDto.ComponentId, cancellationToken);
                if (fan != null)
                {
                    tempBuild.PcBuild_Fans.Add(new PcBuild_Fan
                    {
                        Fan = fan, FanId = fan.Id, PcBuild = tempBuild, Quantity = fanDto.Quantity
                    });
                }
            }

            var ruleResults = _compatibilityChecker.CheckAll(tempBuild);

            var sortedResults = ruleResults
                .OrderBy(r =>
                {
                    if (r.Issues.Any(i => i.Severity == CompatibilitySeverity.Critical)) return 0;
                    if (r.Issues.Any(i => i.Severity == CompatibilitySeverity.Warning)) return 1;
                    if (r.Issues.Any(i => i.Severity == CompatibilitySeverity.Info)) return 2;
                    return 3;
                })
                .ThenBy(r => r.FitnessScore)
                .ToList();

            return Result.Success(BuildCompatibilityReport.From(sortedResults));
        }
    }
}
