using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Interfaces;

namespace PcBuilderApi.Services.Implementations
{
    public class DataCorrectionService : IDataCorrectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DataCorrectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CorrectGpuModels()
        {
            var gpus = await _unitOfWork.Repository<Gpu>().GetAllAsync();
            var gpusWithNullWattage = gpus.Where(g => g.Wattage == null).ToList();
            foreach (var gpu in gpusWithNullWattage)
            {
                string gpuModel = gpu.GpuModel.ToLower();
                var rightGpu = gpus.FirstOrDefault(g => g.GpuModel.ToLower() == gpuModel && g.Wattage != null);
                if (rightGpu != null)
                {
                    gpu.Wattage = rightGpu.Wattage;
                    await _unitOfWork.Repository<Gpu>().UpdateAsync(gpu);
                }
            }
            await _unitOfWork.SaveAsync();
        }
    }
}
