using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class GetBuildByIdQueryHandler : IRequestHandler<GetBuildByIdQuery, PcBuildRequestDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetBuildByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PcBuildRequestDto?> Handle(GetBuildByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var build = await _context.Set<PcBuild>()
                    .Include(b => b.Cpu)
                    .Include(b => b.Gpu)
                    .Include(b => b.Motherboard)
                    .Include(b => b.CpuCooler)
                    .Include(b => b.PcCase)
                    .Include(b => b.PowerSupply)
                    .Include(b => b.PcBuild_Rams)
                    .Include(b => b.PcBuild_Ssds)
                    .Include(b => b.PcBuild_Hdds)
                    .Include(b => b.PcBuild_Fans)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

                if (build == null)
                    return null;

                var cpuInfo = await GetComponentPriceAndStore(build.CpuOfferId, cancellationToken);
                var gpuInfo = await GetComponentPriceAndStore(build.GpuOfferId, cancellationToken);
                var motherboardInfo = await GetComponentPriceAndStore(build.MotherboardOfferId, cancellationToken);
                var cpuCoolerInfo = await GetComponentPriceAndStore(build.CpuCoolerOfferId, cancellationToken);
                var powerSupplyInfo = await GetComponentPriceAndStore(build.PowerSupplyOfferId, cancellationToken);
                var pcCaseInfo = await GetComponentPriceAndStore(build.PcCaseOfferId, cancellationToken);

                var buildDto = new PcBuildRequestDto
                {
                    Id = build.Id,
                    Name = build.Name,
                    Description = build.Description,
                    IsPublished = build.IsPublished,
                    PublishedAt = build.PublishedAt,
                    Price = build.Price,
                    AverageRating = build.AverageRating,
                    CreatedAt = build.CreatedAt,
                    UpdatedAt = build.UpdatedAt,
                    UserId = build.UserId,
                    Username = build.User.Username,
                    AvatarUrl = build.User.AvatarUrl,

                    Cpu = (build.Cpu != null && build.CpuOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.Cpu.Id,
                        OfferId = (Guid)build.CpuOfferId,
                        Name = build.Cpu.Name,
                        Price = cpuInfo.Price,
                        StoreName = cpuInfo.StoreName,
                        ProductOfferUrl = cpuInfo.Url,
                        ImageUrl = build.Cpu.PhotoUrl
                    } : null,

                    Gpu = (build.Gpu != null && build.GpuOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.Gpu.Id,
                        OfferId = (Guid)build.GpuOfferId,
                        Name = build.Gpu.Name,
                        Price = gpuInfo.Price,
                        StoreName = gpuInfo.StoreName,
                        ProductOfferUrl = gpuInfo.Url,
                        ImageUrl = build.Gpu.PhotoUrl
                    } : null,

                    Motherboard = (build.Motherboard != null && build.MotherboardOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.Motherboard.Id,
                        OfferId = (Guid)build.MotherboardOfferId,
                        Name = build.Motherboard.Name,
                        Price = motherboardInfo.Price,
                        StoreName = motherboardInfo.StoreName,
                        ProductOfferUrl = motherboardInfo.Url,
                        ImageUrl = build.Motherboard.PhotoUrl
                    } : null,

                    CpuCooler = (build.CpuCooler != null && build.CpuCoolerOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.CpuCooler.Id,
                        OfferId = (Guid)build.CpuCoolerOfferId,
                        Name = build.CpuCooler.Name,
                        Price = cpuCoolerInfo.Price,
                        StoreName = cpuCoolerInfo.StoreName,
                        ProductOfferUrl = cpuCoolerInfo.Url,
                        ImageUrl = build.CpuCooler.PhotoUrl
                    } : null,

                    PowerSupply = (build.PowerSupply != null && build.PowerSupplyOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.PowerSupply.Id,
                        OfferId = (Guid)build.PowerSupplyOfferId,
                        Name = build.PowerSupply.Name,
                        Price = powerSupplyInfo.Price,
                        StoreName = powerSupplyInfo.StoreName,
                        ProductOfferUrl = powerSupplyInfo.Url,
                        ImageUrl = build.PowerSupply.PhotoUrl
                    } : null,

                    PcCase = (build.PcCase != null && build.PcCaseOfferId != null) ? new ComponentPreviewDto
                    {
                        Id = build.PcCase.Id,
                        OfferId = (Guid)build.PcCaseOfferId,
                        Name = build.PcCase.Name,
                        Price = pcCaseInfo.Price,
                        StoreName = pcCaseInfo.StoreName,
                        ProductOfferUrl = pcCaseInfo.Url,
                        ImageUrl = build.PcCase.PhotoUrl
                    } : null
                };

                buildDto.Rams = await BuildMultiComponentPreviews(
                    build.PcBuild_Rams,
                    r => (r.RamId, r.ProductOfferId, r.Quantity, r.Ram),
                    id => _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == id, cancellationToken),
                    cancellationToken);

                buildDto.Ssds = await BuildMultiComponentPreviews(
                    build.PcBuild_Ssds,
                    s => (s.SsdId, s.ProductOfferId, s.Quantity, s.Ssd),
                    id => _context.Set<Ssd>().FirstOrDefaultAsync(s => s.Id == id, cancellationToken),
                    cancellationToken);

                buildDto.Hdds = await BuildMultiComponentPreviews(
                    build.PcBuild_Hdds,
                    h => (h.HddId, h.ProductOfferId, h.Quantity, h.Hdd),
                    id => _context.Set<Hdd>().FirstOrDefaultAsync(h => h.Id == id, cancellationToken),
                    cancellationToken);

                buildDto.Fans = await BuildMultiComponentPreviews(
                    build.PcBuild_Fans,
                    f => (f.FanId, f.ProductOfferId, f.Quantity, f.Fan),
                    id => _context.Set<Fan>().FirstOrDefaultAsync(f => f.Id == id, cancellationToken),
                    cancellationToken);

                return buildDto;
            }
            catch
            {
                return new PcBuildRequestDto();
            }
        }

        private async Task<List<MultiComponentPreviewDto>> BuildMultiComponentPreviews<TJunction, TComponent>(
            ICollection<TJunction> junctions,
            Func<TJunction, (Guid ComponentId, Guid? OfferId, int Quantity, TComponent? Nav)> extract,
            Func<Guid, Task<TComponent?>> fetchComponent,
            CancellationToken cancellationToken)
            where TComponent : class
        {
            var result = new List<MultiComponentPreviewDto>();

            foreach (var junction in junctions)
            {
                var (componentId, offerId, quantity, nav) = extract(junction);
                if (offerId == null) continue;

                dynamic? component = nav;
                if (component == null)
                {
                    component = await fetchComponent(componentId);
                    if (component == null) continue;
                }

                var info = await GetComponentPriceAndStore(offerId, cancellationToken);

                result.Add(new MultiComponentPreviewDto
                {
                    Id = componentId,
                    OfferId = (Guid)offerId,
                    Name = component.Name,
                    Quantity = quantity,
                    StoreName = info.StoreName,
                    TotalPrice = info.Price * quantity,
                    ProductOfferUrl = info.Url,
                    ImageUrl = component.PhotoUrl
                });
            }

            return result;
        }

        private async Task<(decimal Price, string StoreName, string Url)> GetComponentPriceAndStore(
            Guid? offerId, CancellationToken cancellationToken)
        {
            if (!offerId.HasValue)
                return (0, string.Empty, string.Empty);

            var offer = await _context.Set<ProductOffer>()
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == offerId, cancellationToken);

            if (offer != null)
                return (offer.Price, offer.Store.Name, offer.ProductOfferUrl);

            return (0, string.Empty, string.Empty);
        }
    }
}
