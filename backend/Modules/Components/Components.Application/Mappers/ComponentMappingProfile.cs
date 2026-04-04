using AutoMapper;
using Components.Application.Dtos;
using Components.Application.Dtos.CpuDtos;
using Components.Application.Dtos.CpuCoolerDtos;
using Components.Application.Dtos.FanDtos;
using Components.Application.Dtos.GpuDtos;
using Components.Application.Dtos.HddDtos;
using Components.Application.Dtos.MotherboardDtos;
using Components.Application.Dtos.PcCaseDtos;
using Components.Application.Dtos.PowerSupplyDtos;
using Components.Application.Dtos.RamDtos;
using Components.Application.Dtos.SsdDtos;
using Components.Domain.Entities;

namespace Components.Application.Mappers
{
    public class ComponentMappingProfile : Profile
    {
        public ComponentMappingProfile()
        {
            // ProductOffer / Store (already existed in ProductOfferProfile, consolidated here)
            CreateMap<ProductOffer, ProductOfferDto>()
                .ForMember(dest => dest.Store, opt => opt.MapFrom(src => src.Store));
            CreateMap<Store, StoreDto>();

            // CPU
            CreateMap<Cpu, CpuDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Cpu, CpuListDto>();

            // GPU
            CreateMap<Gpu, GpuDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Gpu, GpuListDto>();
            CreateMap<GpuPowerConnector, GpuPowerConnectorDto>();

            // Motherboard
            CreateMap<Motherboard, MotherboardDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Motherboard, MotherboardListDto>();
            CreateMap<CpuPowerConnector, CpuPowerConnectorDto>();
            CreateMap<PcleSlot, PcleSlotDto>();
            CreateMap<M2Slot, M2SlotDto>();
            CreateMap<RearPort, RearPortDto>();
            CreateMap<InnerPort, InnerPortDto>();

            // RAM
            CreateMap<Ram, RamDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Ram, RamListDto>();

            // CPU Cooler
            CreateMap<CpuCooler, CpuCoolerDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<CpuCooler, CpuCoolerListDto>();
            CreateMap<CpuCoolerSocket, CpuCoolerSocketDto>();

            // PC Case
            CreateMap<PcCase, PcCaseDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<PcCase, PcCaseListDto>();
            CreateMap<PcCaseFormFactor, PcCaseFormFactorDto>();
            CreateMap<PcCaseFanLocation, PcCaseFanLocationDto>();

            // Power Supply — connectors filtered by Type
            CreateMap<PowerSupply, PowerSupplyDto>()
                .ForMember(dest => dest.PowerSupplyMotherboardPowerConnectors,
                    opt => opt.MapFrom(src => src.PowerSupplyPowerConnectors.Where(c => c.Type == "Motherboard")))
                .ForMember(dest => dest.PowerSupplyCpuPowerConnectors,
                    opt => opt.MapFrom(src => src.PowerSupplyPowerConnectors.Where(c => c.Type == "CPU")))
                .ForMember(dest => dest.PowerSupplyGpuPowerConnectors,
                    opt => opt.MapFrom(src => src.PowerSupplyPowerConnectors.Where(c => c.Type == "GPU")))
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<PowerSupply, PowerSupplyListDto>()
                .ForMember(dest => dest.PowerSupplyCpuPowerConnectors,
                    opt => opt.MapFrom(src => src.PowerSupplyPowerConnectors.Where(c => c.Type == "CPU")))
                .ForMember(dest => dest.PowerSupplyGpuPowerConnectors,
                    opt => opt.MapFrom(src => src.PowerSupplyPowerConnectors.Where(c => c.Type == "GPU")));
            CreateMap<PowerSupplyPowerConnector, PowerSupplyMotherboardPowerConnectorDto>();
            CreateMap<PowerSupplyPowerConnector, PowerSupplyCpuPowerConnectorDto>();
            CreateMap<PowerSupplyPowerConnector, PowerSupplyGpuPowerConnectorDto>();

            // SSD
            CreateMap<Ssd, SsdDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Ssd, SsdListDto>();

            // HDD
            CreateMap<Hdd, HddDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Hdd, HddListDto>();

            // Fan
            CreateMap<Fan, FanDto>()
                .ForMember(dest => dest.ProductOffers, opt => opt.Ignore());
            CreateMap<Fan, FanListDto>();
        }
    }
}
