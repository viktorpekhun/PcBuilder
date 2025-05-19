using PcBuilderApi.Models;
using PcBuilderApi.Dtos.CpuDtos;
using static PcBuilderApi.Utilities.SD;
using AutoMapper;
using PcBuilderApi.Dtos;

namespace PcBuilderApi.Mappers.CpuMappers
{
    public class CpuMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Cpu;
        public CpuMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var cpu = (Cpu)entity;
            var offers = productOffers.Where(p => p.ComponentId == cpu.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);

            return new CpuDto
            {
                Id = cpu.Id,
                Name = cpu.Name,
                PhotoUrl = cpu.PhotoUrl,
                Description = cpu.Description,
                Brand = cpu.Brand,
                Socket = cpu.Socket,
                BasicFrequency = cpu.BasicFrequency,
                MaxFrequency = cpu.MaxFrequency,
                Cache = cpu.Cache,
                DimmType = cpu.DimmType,
                Cores = cpu.Cores,
                Threads = cpu.Threads,
                Techprocess = cpu.Techprocess,
                Tdp = cpu.Tdp,
                IntegratedGraphics = cpu.IntegratedGraphics,
                Complectation = cpu.Complectation,
                FactoryLink = cpu.FactoryLink,
                AveragePrice = cpu.AveragePrice,
                OffersCount = cpu.OffersCount,
                ProductOffers = offerDtos
            };
        }
    }
}
