using PcBuilderApi.Mappers;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Interfaces;
using PcBuilderApi.Utilities.Filtering;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Implementations
{
    public class ComponentService : IComponentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IComponentMapper> _detailsMappers;
        private readonly IEnumerable<IComponentListMapper> _listMappers;

        public ComponentService(IUnitOfWork unitOfWork, IEnumerable<IComponentMapper> detailsMappers, 
            IEnumerable<IComponentListMapper> listMappers)
        {
            _unitOfWork = unitOfWork;
            _detailsMappers = detailsMappers;
            _listMappers = listMappers;
        }

        public async Task<PagedResponse<object>> GetAllByTypeAsync(ComponentType componentType, ResourceParameters parameters)
        {
            var mapper = _listMappers.FirstOrDefault(m => m.ComponentType == componentType);
            if (mapper == null) throw new ArgumentException("Unsupported component type");

            // Get filtered, sorted and paged components
            (IEnumerable<object> componentEntities, int totalCount) = componentType switch
            {
                ComponentType.Cpu => await GetFilteredComponentsAndCount<Cpu>(parameters),
                ComponentType.Gpu => await GetFilteredComponentsAndCount<Gpu>(parameters, "GpuPowerConnectors"),
                ComponentType.Motherboard => await GetFilteredComponentsAndCount<Motherboard>(
                    parameters, "CpuPowerConnectors,PcleSlots,M2Slots,RearPorts,InnerPorts"),
                ComponentType.Ram => await GetFilteredComponentsAndCount<Ram>(parameters),
                ComponentType.CpuCooler => await GetFilteredComponentsAndCount<CpuCooler>(
                    parameters, "CpuCoolerSockets"),
                ComponentType.PcCase => await GetFilteredComponentsAndCount<PcCase>(
                    parameters, "PcCaseFormFactors,PcCaseFanLocations"),
                ComponentType.PowerSupply => await GetFilteredComponentsAndCount<PowerSupply>(
                    parameters, "PowerSupplyPowerConnectors"),
                ComponentType.Ssd => await GetFilteredComponentsAndCount<Ssd>(parameters),
                ComponentType.Hdd => await GetFilteredComponentsAndCount<Hdd>(parameters),
                ComponentType.Fan => await GetFilteredComponentsAndCount<Fan>(parameters),
                _ => throw new ArgumentException("Unsupported component type")
            };

            var mappedItems = mapper.MapAll(componentEntities).ToList();

            return new PagedResponse<object>(mappedItems, totalCount, parameters);
        }

        public async Task<object> GetByIdAsync(Guid id, ComponentType componentType)
        {
            var mapper = _detailsMappers.FirstOrDefault(m => m.ComponentType == componentType);
            if (mapper == null) throw new ArgumentException("Unsupported component type");

            object entity = componentType switch
            {
                ComponentType.Cpu => await _unitOfWork.Repository<Cpu>()
                    .GetFirstOrDefaultAsync(c => c.Id == id),
                ComponentType.Gpu => await _unitOfWork.Repository<Gpu>()
                    .GetFirstOrDefaultAsync(g => g.Id == id, includeProperties: "GpuPowerConnectors"),
                ComponentType.Motherboard => await _unitOfWork.Repository<Motherboard>()
                    .GetFirstOrDefaultAsync(m => m.Id == id, includeProperties: "CpuPowerConnectors,PcleSlots,M2Slots,RearPorts,InnerPorts"),
                ComponentType.Ram => await _unitOfWork.Repository<Ram>()
                    .GetFirstOrDefaultAsync(r => r.Id == id),
                ComponentType.CpuCooler => await _unitOfWork.Repository<CpuCooler>()
                    .GetFirstOrDefaultAsync(c => c.Id == id, includeProperties: "CpuCoolerSockets"),
                ComponentType.PcCase => await _unitOfWork.Repository<PcCase>()
                    .GetFirstOrDefaultAsync(c => c.Id == id, includeProperties: "PcCaseFormFactors,PcCaseFanLocations"),
                ComponentType.PowerSupply => await _unitOfWork.Repository<PowerSupply>()
                    .GetFirstOrDefaultAsync(ps => ps.Id == id, includeProperties: "PowerSupplyPowerConnectors"),
                ComponentType.Ssd => await _unitOfWork.Repository<Ssd>()
                    .GetFirstOrDefaultAsync(s => s.Id == id),
                ComponentType.Hdd => await _unitOfWork.Repository<Hdd>()
                    .GetFirstOrDefaultAsync(h => h.Id == id),
                ComponentType.Fan => await _unitOfWork.Repository<Fan>()
                    .GetFirstOrDefaultAsync(f => f.Id == id),
                _ => throw new ArgumentException("Unsupported component type")
            };

            if (entity == null)
                throw new KeyNotFoundException($"Component with ID {id} not found for type {componentType}");

            var offers = await _unitOfWork.Repository<ProductOffer>().GetAllAsync(p => p.ComponentId == id && p.ComponentType == componentType, includeProperties: "Store");

            return mapper.MapById(entity, offers);
        }

        public async Task<Dictionary<string, List<string>>> GetFilterOptionsAsync(ComponentType componentType)
        {
            var result = new Dictionary<string, List<string>>();
            switch (componentType)
            {
                case ComponentType.Cpu:
                    {
                        var components = await _unitOfWork.Repository<Cpu>().GetAllAsync();

                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;
                        var sockets = components.Select(c => c.Socket).Distinct().OrderBy(b => b).ToList();
                        result["socket"] = sockets;
                        var basicFrequencyMin = components.Min(c => c.BasicFrequency);
                        var basicFrequencyMax = components.Max(c => c.BasicFrequency);
                        result["basicFrequency_range"] = new List<string> {
                                basicFrequencyMin?.ToString("0.0") ?? "1.0",
                                basicFrequencyMax?.ToString("0.0") ?? "5.0"
                            };
                        var maxFrequencyMin = components.Min(c => c.MaxFrequency);
                        var maxFrequencyMax = components.Max(c => c.MaxFrequency);
                        result["maxFrequency_range"] = new List<string> {
                                maxFrequencyMin?.ToString("0.0") ?? "1.0",
                                maxFrequencyMax?.ToString("0.0") ?? "5.0"
                            };
                        var coresMin = components.Min(c => c.Cores);
                        var coresMax = components.Max(c => c.Cores);
                        result["cores_range"] = new List<string> {
                                coresMin?.ToString() ?? "1",
                                coresMax?.ToString() ?? "128"
                            };
                        var threadsMin = components.Min(c => c.Threads);
                        var threadsMax = components.Max(c => c.Threads);
                        result["threads_range"] = new List<string> {
                                threadsMin?.ToString() ?? "1",
                                threadsMax?.ToString() ?? "256"
                            };

                        var cacheMin = components.Min(c => c.Cache);
                        var cacheMax = components.Max(c => c.Cache);
                        result["cache_range"] = new List<string> {
                                cacheMin?.ToString() ?? "1",
                                cacheMax?.ToString() ?? "128"
                            };

                        var complectations = components.Select(c => c.Complectation).Distinct().OrderBy(b => b).ToList();
                        result["complectation"] = complectations.Where(c => c != null).Cast<string>().ToList();

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };
                        break;
                    }
                case ComponentType.Gpu:
                    {
                        var components = await _unitOfWork.Repository<Gpu>().GetAllAsync();

                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;
                        var gpuManufacturers = components.Select(c => c.GpuManufacturer).Distinct().OrderBy(b => b).ToList();
                        result["gpuManufacturer"] = gpuManufacturers;
                        var gpuModels = components.Select(c => c.GpuModel).Distinct().OrderBy(b => b).ToList();
                        result["gpuModel"] = gpuModels;

                        var memoryTypes = components.Select(c => c.MemoryType).Distinct().OrderBy(b => b).ToList();
                        result["memoryType"] = memoryTypes.Where(mt => mt != null).Cast<string>().ToList();

                        var memoryMin = components.Min(c => c.Memory);
                        var memoryMax = components.Max(c => c.Memory);
                        result["memory_range"] = new List<string> {
                                memoryMin?.ToString() ?? "1",
                                memoryMax?.ToString() ?? "64"
                            };

                        var maxFrequencyMin = components.Min(c => c.MaxFrequency);
                        var maxFrequencyMax = components.Max(c => c.MaxFrequency);
                        result["maxFrequency_range"] = new List<string> {
                                maxFrequencyMin?.ToString() ?? "1",
                                maxFrequencyMax?.ToString() ?? "5000"
                            };
                        var cudaCoresMin = components.Min(c => c.CudaCores);
                        var cudaCoresMax = components.Max(c => c.CudaCores);
                        result["cudaCores_range"] = new List<string> {
                                cudaCoresMin?.ToString() ?? "1",
                                cudaCoresMax?.ToString() ?? "10000"
                            };

                        var memoryBuses = components.Select(c => c.MemoryBus).Distinct().OrderBy(b => b).ToList();
                        result["memoryBus"] = memoryBuses.Where(mb => mb.HasValue).Select(mb => mb.Value.ToString()).ToList();

                        var pcleVersions = components.Select(c => c.PcleVersion).Distinct().OrderBy(b => b).ToList();
                        result["pcleVersion"] = pcleVersions.Where(pv => pv.HasValue).Select(pv => pv.Value.ToString("0.0")).ToList();

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };

                        break;
                    }
                case ComponentType.Motherboard:
                    {
                        var components = await _unitOfWork.Repository<Motherboard>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;
                        var sockets = components.Select(c => c.Socket).Distinct().OrderBy(b => b).ToList();
                        result["socket"] = sockets;
                        var chipsets = components.Select(c => c.Chipset).Distinct().OrderBy(b => b).ToList();
                        result["chipset"] = chipsets;

                        var dimmSlots = components.Select(c => c.DimmSlots).Distinct().OrderBy(b => b).ToList();
                        result["dimmSlots"] = dimmSlots.Where(ds => ds.HasValue).Select(ds => ds.Value.ToString()).ToList();

                        var dimmTypes = components.Select(c => c.DimmType).Distinct().OrderBy(b => b).ToList();
                        result["dimmType"] = dimmTypes.Where(dt => dt != null).Cast<string>().ToList();

                        var dimmCapacities = components.Select(c => c.DimmCapacity).Distinct().OrderBy(b => b).ToList();
                        result["dimmCapacity"] = dimmCapacities.Where(dc => dc.HasValue).Select(dc => dc.Value.ToString()).ToList();

                        var sata3CountMin = components.Min(c => c.Sata3Count);
                        var sata3CountMax = components.Max(c => c.Sata3Count);
                        result["sata3Count_range"] = new List<string> {
                                sata3CountMin?.ToString() ?? "0",
                                sata3CountMax?.ToString() ?? "10"
                            };

                        var formFactors = components.Select(c => c.FormFactor).Distinct().OrderBy(b => b).ToList();
                        result["formFactor"] = formFactors.Where(ff => ff != null).Cast<string>().ToList();

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };

                        break;
                    }
                case ComponentType.CpuCooler:
                    {
                        var components = await _unitOfWork.Repository<CpuCooler>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;

                        var fanSizes = components.Select(c => c.FanSize).Distinct().OrderBy(b => b).ToList();
                        result["fanSize"] = fanSizes.Where(fs => fs.HasValue).Select(fs => fs.Value.ToString("0.0")).ToList();

                        var fanCounts = components.Select(c => c.FanCount).Distinct().OrderBy(b => b).ToList();
                        result["fanCount"] = fanCounts.Where(fc => fc.HasValue).Select(fc => fc.Value.ToString()).ToList();

                        var heightMin = components.Min(c => c.Height);
                        var heightMax = components.Max(c => c.Height);
                        result["height_range"] = new List<string> {
                                heightMin?.ToString("0.0") ?? "1.0",
                                heightMax?.ToString("0.0") ?? "20.0"
                            };
                        var maxPowerDissipationMin = components.Min(c => c.MaxPowerDissipation);
                        var maxPowerDissipationMax = components.Max(c => c.MaxPowerDissipation);
                        result["maxPowerDissipation_range"] = new List<string> {
                                maxPowerDissipationMin?.ToString() ?? "1",
                                maxPowerDissipationMax?.ToString() ?? "500"
                            };

                        var maxSpeedMin = components.Min(c => c.MaxSpeed);
                        var maxSpeedMax = components.Max(c => c.MaxSpeed);
                        result["maxSpeed_range"] = new List<string> {
                                maxSpeedMin?.ToString() ?? "1",
                                maxSpeedMax?.ToString() ?? "5000"
                            };

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };

                        break;
                    }
                case ComponentType.PowerSupply:
                    {
                        var components = await _unitOfWork.Repository<PowerSupply>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;

                        var formFactors = components.Select(c => c.FormFactor).Distinct().OrderBy(b => b).ToList();
                        result["formFactor"] = formFactors.Where(ff => ff != null).Cast<string>().ToList();

                        var wattageMin = components.Min(c => c.Wattage);
                        var wattageMax = components.Max(c => c.Wattage);
                        result["wattage_range"] = new List<string> {
                                wattageMin.ToString() ?? "1",
                                wattageMax.ToString() ?? "2000"
                            };
                        var efficiencyStandarts = components.Select(c => c.EfficiencyStandart).Distinct().OrderBy(b => b).ToList();
                        result["efficiencyStandart"] = efficiencyStandarts.Where(es => es != null).Cast<string>().ToList();

                        var efficiencyPercentMin = components.Min(c => c.EfficiencyPercent);
                        var efficiencyPercentMax = components.Max(c => c.EfficiencyPercent);
                        result["efficiencyPercent_range"] = new List<string> {
                                efficiencyPercentMin?.ToString("0.0") ?? "1.0",
                                efficiencyPercentMax?.ToString("0.0") ?? "100.0"
                            };

                        var molexCountMin = components.Min(c => c.MolexCount);
                        var molexCountMax = components.Max(c => c.MolexCount);
                        result["molexCount_range"] = new List<string> {
                                molexCountMin?.ToString() ?? "0",
                                molexCountMax?.ToString() ?? "20"
                            };

                        var sataCountMin = components.Min(c => c.SataCount);
                        var sataCountMax = components.Max(c => c.SataCount);
                        result["sataCount_range"] = new List<string> {
                                sataCountMin?.ToString() ?? "0",
                                sataCountMax?.ToString() ?? "20"
                            };

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };

                        break;
                    }
                case ComponentType.PcCase:
                    {
                        var components = await _unitOfWork.Repository<PcCase>().GetAllAsync();

                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;

                        var sizeStandards = components.Select(c => c.SizeStandard).Distinct().OrderBy(b => b).ToList();
                        result["sizeStandard"] = sizeStandards.Where(ss => ss != null).Cast<string>().ToList();

                        var weightMin = components.Min(c => c.Weight);
                        var weightMax = components.Max(c => c.Weight);
                        result["weight_range"] = new List<string> {
                                weightMin?.ToString("0.0") ?? "1.0",
                                weightMax?.ToString("0.0") ?? "20.0"
                            };

                        var maxGpuLengthMin = components.Min(c => c.MaxGpuLength);
                        var maxGpuLengthMax = components.Max(c => c.MaxGpuLength);
                        result["maxGpuLength_range"] = new List<string> {
                                maxGpuLengthMin?.ToString("0.0") ?? "1.0",
                                maxGpuLengthMax?.ToString("0.0") ?? "500.0"
                            };
                        var maxCpuCoolerHeightMin = components.Min(c => c.MaxCpuCoolerHeight);
                        var maxCpuCoolerHeightMax = components.Max(c => c.MaxCpuCoolerHeight);
                        result["maxCpuCoolerHeight_range"] = new List<string> {
                                maxCpuCoolerHeightMin?.ToString("0.0") ?? "1.0",
                                maxCpuCoolerHeightMax?.ToString("0.0") ?? "20.0"
                            };

                        var slot25QuantMin = components.Min(c => c.Slot25Quant);
                        var slot25QuantMax = components.Max(c => c.Slot25Quant);
                        result["slot25Quant_range"] = new List<string> {
                                slot25QuantMin?.ToString() ?? "0",
                                slot25QuantMax?.ToString() ?? "20"
                            };

                        var slot35QuantMin = components.Min(c => c.Slot35Quant);
                        var slot35QuantMax = components.Max(c => c.Slot35Quant);
                        result["slot35Quant_range"] = new List<string> {
                                slot35QuantMin?.ToString() ?? "0",
                                slot35QuantMax?.ToString() ?? "20"
                            };

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };
                        break;
                    }
                case ComponentType.Ram:
                    {
                        var components = await _unitOfWork.Repository<Ram>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;

                        var types = components.Select(c => c.Type).Distinct().OrderBy(b => b).ToList();
                        result["type"] = types.Where(t => t != null).Cast<string>().ToList();

                        var frequencyMin = components.Min(c => c.Frequency);
                        var frequencyMax = components.Max(c => c.Frequency);
                        result["frequency_range"] = new List<string> {
                                frequencyMin.ToString() ?? "1",
                                frequencyMax.ToString() ?? "5000"
                            };

                        var capacityMin = components.Min(c => c.Capacity);
                        var capacityMax = components.Max(c => c.Capacity);
                        result["capacity_range"] = new List<string> {
                                capacityMin?.ToString() ?? "1",
                                capacityMax?.ToString() ?? "128"
                            };

                        var moduleQuantities = components.Select(c => c.ModuleQuantity).Distinct().OrderBy(b => b).ToList();
                        result["moduleQuantity"] = moduleQuantities.Where(mq => mq.HasValue).Select(mq => mq.Value.ToString()).ToList();

                        var bufferizationTypes = components.Select(c => c.Bufferization).Distinct().OrderBy(b => b).ToList();
                        result["bufferization"] = bufferizationTypes.Where(bt => bt != null).Cast<string>().ToList();

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };
                        break;
                    }
                case ComponentType.Ssd:
                    {
                        var components = await _unitOfWork.Repository<Ssd>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;

                        var formFactors = components.Select(c => c.FormFactor).Distinct().OrderBy(b => b).ToList();
                        result["formFactor"] = formFactors.Where(ff => ff != null).Cast<string>().ToList();

                        var capacityMin = components.Min(c => c.Capacity);
                        var capacityMax = components.Max(c => c.Capacity);
                        result["capacity_range"] = new List<string> {
                                capacityMin.ToString() ?? "1",
                                capacityMax.ToString() ?? "10000"
                            };

                        var maxReadSpeedMin = components.Min(c => c.MaxReadSpeed);
                        var maxReadSpeedMax = components.Max(c => c.MaxReadSpeed);
                        result["maxReadSpeed_range"] = new List<string> {
                                maxReadSpeedMin?.ToString() ?? "1",
                                maxReadSpeedMax?.ToString() ?? "10000"
                            };
                        var maxWriteSpeedMin = components.Min(c => c.MaxWriteSpeed);
                        var maxWriteSpeedMax = components.Max(c => c.MaxWriteSpeed);
                        result["maxWriteSpeed_range"] = new List<string> {
                                maxWriteSpeedMin?.ToString() ?? "1",
                                maxWriteSpeedMax?.ToString() ?? "10000"
                            };

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };
                        break;
                    }
                case ComponentType.Hdd:
                    {
                        var components = await _unitOfWork.Repository<Hdd>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;
                        var formFactors = components.Select(c => c.FormFactor).Distinct().OrderBy(b => b).ToList();
                        result["formFactor"] = formFactors.Where(ff => ff != null).Cast<string>().ToList();
                        var capacityMin = components.Min(c => c.Capacity);
                        var capacityMax = components.Max(c => c.Capacity);
                        result["capacity_range"] = new List<string> {
                                capacityMin.ToString() ?? "1",
                                capacityMax.ToString() ?? "10000"
                            };

                        var spindleSpeeds = components.Select(c => c.SpindleSpeed).Distinct().OrderBy(b => b).ToList();
                        result["spindleSpeed"] = spindleSpeeds.Where(ss => ss.HasValue).Select(ss => ss.Value.ToString()).ToList();

                        var cacheSizes = components.Select(c => c.Cache).Distinct().OrderBy(b => b).ToList();
                        result["cache"] = cacheSizes.Where(cs => cs.HasValue).Select(cs => cs.Value.ToString()).ToList();

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };
                        break;
                    }
                case ComponentType.Fan:
                    {
                        var components = await _unitOfWork.Repository<Fan>().GetAllAsync();
                        var brands = components.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
                        result["brand"] = brands;
                        
                        var moduleCounts = components.Select(c => c.ModuleCount).Distinct().OrderBy(b => b).ToList();
                        result["moduleCount"] = moduleCounts.Where(mc => mc.HasValue).Select(mc => mc.Value.ToString()).ToList();

                        var maxSpeedMin = components.Min(c => c.MaxSpeed);
                        var maxSpeedMax = components.Max(c => c.MaxSpeed);
                        result["maxSpeed_range"] = new List<string> {
                                maxSpeedMin?.ToString() ?? "1",
                                maxSpeedMax?.ToString() ?? "5000"
                            };

                        var airflowMin = components.Min(c => c.AirflowCfm);
                        var airflowMax = components.Max(c => c.AirflowCfm);
                        result["airflowCfm_range"] = new List<string> {
                                airflowMin?.ToString("0.0") ?? "1.0",
                                airflowMax?.ToString("0.0") ?? "100.0"
                            };

                        var noiseLevelMin = components.Min(c => c.NoiseLevelDb);
                        var noiseLevelMax = components.Max(c => c.NoiseLevelDb);
                        result["noiseLevelDb_range"] = new List<string> {
                                noiseLevelMin?.ToString("0.0") ?? "1.0",
                                noiseLevelMax?.ToString("0.0") ?? "100.0"
                            };

                        var sizeLengthMin = components.Min(c => c.SizeLength);
                        var sizeLengthMax = components.Max(c => c.SizeLength);
                        result["sizeLength_range"] = new List<string> {
                                sizeLengthMin?.ToString("0.0") ?? "1.0",
                                sizeLengthMax?.ToString("0.0") ?? "500.0"
                            };

                        var averagePriceMin = components.Min(c => c.AveragePrice);
                        var averagePriceMax = components.Max(c => c.AveragePrice);
                        result["averagePrice_range"] = new List<string> {
                                averagePriceMin?.ToString("0.00") ?? "1.00",
                                averagePriceMax?.ToString("0.00") ?? "1000.00"
                            };
                        break;
                    }
            }
            foreach (var key in result.Keys.ToList())
            {
                result[key].RemoveAll(string.IsNullOrEmpty);
            }
            return result;
        }

        private async Task<(IEnumerable<object>, int)> GetFilteredComponentsAndCount<T>(
            ResourceParameters parameters,
            string includeProperties = "") where T : class
        {
            var repository = _unitOfWork.Repository<T>();

            var result = await repository.GetFilteredAndPagedAsync(
                parameters: parameters,
                includeProperties: includeProperties
            );

            return (result.items.Cast<object>(), result.totalCount);
        }


    }
}
