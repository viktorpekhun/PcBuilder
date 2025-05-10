using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;

namespace PcBuilderApi.Mappers.ProductOfferMappers
{
    public class ProductOfferProfile : Profile
    {
        public ProductOfferProfile()
        {
            CreateMap<ProductOffer, ProductOfferDto>()
                .ForMember(dest => dest.Store, opt => opt.MapFrom(src => src.Store));
            CreateMap<Store, StoreDto>();
        }
    }
}
