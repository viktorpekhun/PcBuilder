using PcBuilderApi.Dtos.PcCaseDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.PcCaseMappers
{
    public class PcCaseListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.PcCase;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var pcCases = entities.Cast<PcCase>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);
            return pcCases.Select(pcCase =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == pcCase.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;
                var pcCaseFormFactorDtos = pcCase.PcCaseFormFactors
                     .Select(c => new PcCaseFormFactorDto
                     {
                         Name = c.Name
                     })
                     .ToList();
                return new PcCaseListDto
                {
                    Id = pcCase.Id,
                    Name = pcCase.Name,
                    PhotoUrl = pcCase.PhotoUrl,
                    Brand = pcCase.Brand,
                    SizeStandard = pcCase.SizeStandard,
                    SizeDimentions = pcCase.SizeDimentions,
                    PsuWattage = pcCase.PsuWattage,
                    PsuLocation = pcCase.PsuLocation,
                    HasDustFilters = pcCase.HasDustFilters,
                    Usb = pcCase.Usb,
                    AveragePrice = avgPrice,
                    PcCaseFormFactors = pcCaseFormFactorDtos
                };
            });
        }
    }
}
