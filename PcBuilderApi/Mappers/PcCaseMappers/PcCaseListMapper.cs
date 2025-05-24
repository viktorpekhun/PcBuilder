using PcBuilderApi.Dtos.PcCaseDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.PcCaseMappers
{
    public class PcCaseListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.PcCase;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var pcCases = entities.Cast<PcCase>();
            return pcCases.Select(pcCase =>
            {
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
                    PsuLocation = pcCase.PsuLocation,
                    Usb = pcCase.Usb,
                    AveragePrice = pcCase.AveragePrice,
                    OffersCount = pcCase.OffersCount,
                    PcCaseFormFactors = pcCaseFormFactorDtos
                };
            });
        }
    }
}
