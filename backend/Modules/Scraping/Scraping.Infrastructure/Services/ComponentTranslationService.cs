using Components.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Services
{
    public class ComponentTranslationService : IComponentTranslationService
    {
        private readonly IApplicationDbContext _context;
        private readonly ITranslationService _translationService;

        public ComponentTranslationService(
            IApplicationDbContext context,
            ITranslationService translationService)
        {
            _context = context;
            _translationService = translationService;
        }


        public async Task TranslatePcCaseFieldsAsync(CancellationToken cancellationToken = default)
        {
            var pcCases = await _context.Set<PcCase>()
                .Include(p => p.PcCaseFanLocations)
                .Where(p =>
                    (p.PsuLocation != null && p.PsuLocation.En == null) ||
                    (p.BuiltInFans != null && p.BuiltInFans.En == null) ||
                    (p.AdditionalFanPlaces != null && p.AdditionalFanPlaces.En == null) ||
                    p.PcCaseFanLocations.Any(fl => fl.Name.En == null))
                .ToListAsync(cancellationToken);

            if (pcCases.Count == 0) return;

            var textSet = new HashSet<string>();

            foreach (var pc in pcCases)
            {
                if (pc.PsuLocation != null && pc.PsuLocation.En == null && !string.IsNullOrEmpty(pc.PsuLocation.Uk))
                    textSet.Add(pc.PsuLocation.Uk);

                if (pc.BuiltInFans != null && pc.BuiltInFans.En == null && !string.IsNullOrEmpty(pc.BuiltInFans.Uk))
                    textSet.Add(pc.BuiltInFans.Uk);

                if (pc.AdditionalFanPlaces != null && pc.AdditionalFanPlaces.En == null && !string.IsNullOrEmpty(pc.AdditionalFanPlaces.Uk))
                    textSet.Add(pc.AdditionalFanPlaces.Uk);

                foreach (var fl in pc.PcCaseFanLocations)
                {
                    if (fl.Name.En == null && !string.IsNullOrEmpty(fl.Name.Uk))
                        textSet.Add(fl.Name.Uk);
                }
            }

            var textsToTranslate = textSet.ToList();
            if (textsToTranslate.Count == 0) return;

            var translations = await _translationService.TranslateBatchAsync(
                textsToTranslate, "uk", "en", cancellationToken);

            var translationMap = new Dictionary<string, string>();
            for (int i = 0; i < textsToTranslate.Count; i++)
            {
                if (!string.IsNullOrEmpty(translations[i]))
                    translationMap[textsToTranslate[i]] = translations[i];
            }

            foreach (var pc in pcCases)
            {
                if (pc.PsuLocation != null && pc.PsuLocation.En == null
                    && !string.IsNullOrEmpty(pc.PsuLocation.Uk)
                    && translationMap.TryGetValue(pc.PsuLocation.Uk, out var psuTrans))
                {
                    pc.PsuLocation.En = psuTrans;
                }

                if (pc.BuiltInFans != null && pc.BuiltInFans.En == null
                    && !string.IsNullOrEmpty(pc.BuiltInFans.Uk)
                    && translationMap.TryGetValue(pc.BuiltInFans.Uk, out var fansTrans))
                {
                    pc.BuiltInFans.En = fansTrans;
                }

                if (pc.AdditionalFanPlaces != null && pc.AdditionalFanPlaces.En == null
                    && !string.IsNullOrEmpty(pc.AdditionalFanPlaces.Uk)
                    && translationMap.TryGetValue(pc.AdditionalFanPlaces.Uk, out var additionalPlacesTrans))
                {
                    pc.AdditionalFanPlaces.En = additionalPlacesTrans;
                }

                foreach (var fl in pc.PcCaseFanLocations)
                {
                    if (fl.Name.En == null && !string.IsNullOrEmpty(fl.Name.Uk)
                        && translationMap.TryGetValue(fl.Name.Uk, out var nameTrans))
                    {
                        fl.Name.En = nameTrans;
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task TranslateFanFieldsAsync(CancellationToken cancellationToken = default)
        {
            var fans = await _context.Set<Fan>()
                .Where(p =>
                    (p.BearingType != null && p.BearingType.En == null) ||
                    (p.Color != null && p.Color.En == null))
                .ToListAsync(cancellationToken);

            if (fans.Count == 0) return;

            var textSet = new HashSet<string>();

            foreach (var fan in fans)
            {
                if (fan.BearingType != null && fan.BearingType.En == null && !string.IsNullOrEmpty(fan.BearingType.Uk))
                    textSet.Add(fan.BearingType.Uk);

                if (fan.Color != null && fan.Color.En == null && !string.IsNullOrEmpty(fan.Color.Uk))
                    textSet.Add(fan.Color.Uk);

            }

            var textsToTranslate = textSet.ToList();
            if (textsToTranslate.Count == 0) return;

            var translations = await _translationService.TranslateBatchAsync(
                textsToTranslate, "uk", "en", cancellationToken);

            var translationMap = new Dictionary<string, string>();
            for (int i = 0; i < textsToTranslate.Count; i++)
            {
                if (!string.IsNullOrEmpty(translations[i]))
                    translationMap[textsToTranslate[i]] = translations[i];
            }

            foreach (var fan in fans)
            {
                if (fan.BearingType != null && fan.BearingType.En == null
                    && !string.IsNullOrEmpty(fan.BearingType.Uk)
                    && translationMap.TryGetValue(fan.BearingType.Uk, out var psuTrans))
                {
                    fan.BearingType.En = psuTrans;
                }

                if (fan.Color != null && fan.Color.En == null
                    && !string.IsNullOrEmpty(fan.Color.Uk)
                    && translationMap.TryGetValue(fan.Color.Uk, out var fansTrans))
                {
                    fan.Color.En = fansTrans;
                }

            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task TranslatePowerSupplyFieldsAsync(CancellationToken cancellationToken = default)
        {
            var powerSupplies = await _context.Set<PowerSupply>()
                .Where(p =>
                    (p.Modularity != null && p.Modularity.En == null))
                .ToListAsync(cancellationToken);

            if (powerSupplies.Count == 0) return;

            var textSet = new HashSet<string>();

            foreach (var ps in powerSupplies)
            {
                if (ps.Modularity != null && ps.Modularity.En == null && !string.IsNullOrEmpty(ps.Modularity.Uk))
                    textSet.Add(ps.Modularity.Uk);
            }

            var textsToTranslate = textSet.ToList();
            if (textsToTranslate.Count == 0) return;

            var translations = await _translationService.TranslateBatchAsync(
                textsToTranslate, "uk", "en", cancellationToken);

            var translationMap = new Dictionary<string, string>();
            for (int i = 0; i < textsToTranslate.Count; i++)
            {
                if (!string.IsNullOrEmpty(translations[i]))
                    translationMap[textsToTranslate[i]] = translations[i];
            }

            foreach (var ps in powerSupplies)
            {
                if (ps.Modularity != null && ps.Modularity.En == null
                    && !string.IsNullOrEmpty(ps.Modularity.Uk)
                    && translationMap.TryGetValue(ps.Modularity.Uk, out var psuTrans))
                {
                    ps.Modularity.En = psuTrans;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task TranslateRamFieldsAsync(CancellationToken cancellationToken = default)
        {
            var rams = await _context.Set<Ram>()
                .Where(p =>
                    (p.Color != null && p.Color.En == null))
                .ToListAsync(cancellationToken);

            if (rams.Count == 0) return;

            var textSet = new HashSet<string>();

            foreach (var ram in rams)
            {
                if (ram.Color != null && ram.Color.En == null && !string.IsNullOrEmpty(ram.Color.Uk))
                    textSet.Add(ram.Color.Uk);
            }

            var textsToTranslate = textSet.ToList();
            if (textsToTranslate.Count == 0) return;

            var translations = await _translationService.TranslateBatchAsync(
                textsToTranslate, "uk", "en", cancellationToken);

            var translationMap = new Dictionary<string, string>();
            for (int i = 0; i < textsToTranslate.Count; i++)
            {
                if (!string.IsNullOrEmpty(translations[i]))
                    translationMap[textsToTranslate[i]] = translations[i];
            }

            foreach (var ram in rams)
            {
                if (ram.Color != null && ram.Color.En == null
                    && !string.IsNullOrEmpty(ram.Color.Uk)
                    && translationMap.TryGetValue(ram.Color.Uk, out var psuTrans))
                {
                    ram.Color.En = psuTrans;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task TranslateCpuCoolerFieldsAsync(CancellationToken cancellationToken = default)
        {
            var cpuCoolers = await _context.Set<CpuCooler>()
                .Where(p =>
                    (p.RadiatorMaterial != null && p.RadiatorMaterial.En == null))
                .ToListAsync(cancellationToken);

            if (cpuCoolers.Count == 0) return;

            var textSet = new HashSet<string>();

            foreach (var cpuCooler in cpuCoolers)
            {
                if (cpuCooler.RadiatorMaterial != null && cpuCooler.RadiatorMaterial.En == null && !string.IsNullOrEmpty(cpuCooler.RadiatorMaterial.Uk))
                    textSet.Add(cpuCooler.RadiatorMaterial.Uk);
            }

            var textsToTranslate = textSet.ToList();
            if (textsToTranslate.Count == 0) return;

            var translations = await _translationService.TranslateBatchAsync(
                textsToTranslate, "uk", "en", cancellationToken);

            var translationMap = new Dictionary<string, string>();
            for (int i = 0; i < textsToTranslate.Count; i++)
            {
                if (!string.IsNullOrEmpty(translations[i]))
                    translationMap[textsToTranslate[i]] = translations[i];
            }

            foreach (var cpuCooler in cpuCoolers)
            {
                if (cpuCooler.RadiatorMaterial != null && cpuCooler.RadiatorMaterial.En == null
                    && !string.IsNullOrEmpty(cpuCooler.RadiatorMaterial.Uk)
                    && translationMap.TryGetValue(cpuCooler.RadiatorMaterial.Uk, out var psuTrans))
                {
                    cpuCooler.RadiatorMaterial.En = psuTrans;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
