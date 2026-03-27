using Components.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Services
{
    public class DataCorrectionService : IDataCorrectionService
    {
        private readonly IApplicationDbContext _context;

        public DataCorrectionService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CorrectGpuModels()
        {
            var gpus = await _context.Set<Gpu>().ToListAsync();
            var gpusWithNullWattage = gpus.Where(g => g.Wattage == null).ToList();
            foreach (var gpu in gpusWithNullWattage)
            {
                string gpuModel = gpu.GpuModel.ToLower();
                var rightGpu = gpus.FirstOrDefault(g => g.GpuModel.ToLower() == gpuModel && g.Wattage != null);
                if (rightGpu != null)
                {
                    gpu.Wattage = rightGpu.Wattage;
                    _context.Set<Gpu>().Update(gpu);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
