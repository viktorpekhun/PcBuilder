using AutoMapper;
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
using Components.Application.Queries;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class GetSimilarComponentsHandler : IRequestHandler<GetSimilarComponentsQuery, Result<List<object>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSimilarComponentsHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<object>>> Handle(GetSimilarComponentsQuery request, CancellationToken cancellationToken)
        {
            return request.ComponentType switch
            {
                ComponentType.Cpu => await GetSimilarWithSocket<Cpu, CpuListDto>(
                    request.Id, request.Count, cancellationToken, c => c.Socket),

                ComponentType.Gpu => await GetSimilar<Gpu, GpuListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.Motherboard => await GetSimilarWithSocket<Motherboard, MotherboardListDto>(
                    request.Id, request.Count, cancellationToken, m => m.Socket),

                ComponentType.Ram => await GetSimilar<Ram, RamListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.CpuCooler => await GetSimilar<CpuCooler, CpuCoolerListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.PcCase => await GetSimilar<PcCase, PcCaseListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.PowerSupply => await GetSimilar<PowerSupply, PowerSupplyListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.Ssd => await GetSimilar<Ssd, SsdListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.Hdd => await GetSimilar<Hdd, HddListDto>(
                    request.Id, request.Count, cancellationToken),

                ComponentType.Fan => await GetSimilar<Fan, FanListDto>(
                    request.Id, request.Count, cancellationToken),

                _ => Result<List<object>>.Failure(new Error("NOT_FOUND", "Component type not supported", 400))
            };
        }

        private async Task<Result<List<object>>> GetSimilar<TEntity, TDto>(
            Guid id,
            int count,
            CancellationToken ct)
            where TEntity : class, IHasAveragePrice
            where TDto : class
        {
            var baseComponent = await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
            if (baseComponent == null)
                return Result<List<object>>.Failure(new Error("NOT_FOUND", "Component not found", 404));

            decimal basePrice = baseComponent.AveragePrice ?? 0;
            decimal minPrice = basePrice * 0.7m;
            decimal maxPrice = basePrice * 1.3m;

            var candidates = await _context.Set<TEntity>()
                .Where(e => e.Id != id && e.AveragePrice >= minPrice && e.AveragePrice <= maxPrice)
                .ToListAsync(ct);

            var result = candidates
                .OrderBy(e => Math.Abs((e.AveragePrice ?? 0) - basePrice))
                .Take(count)
                .ToList();

            return Result<List<object>>.Success(_mapper.Map<List<TDto>>(result).Cast<object>().ToList());
        }

        private async Task<Result<List<object>>> GetSimilarWithSocket<TEntity, TDto>(
            Guid id,
            int count,
            CancellationToken ct,
            Func<TEntity, string> socketSelector)
            where TEntity : class, IHasAveragePrice
            where TDto : class
        {
            var baseComponent = await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);
            if (baseComponent == null)
                return Result<List<object>>.Failure(new Error("NOT_FOUND", "Component not found", 404));

            decimal basePrice = baseComponent.AveragePrice ?? 0;
            decimal minPrice = basePrice * 0.7m;
            decimal maxPrice = basePrice * 1.3m;
            string baseSocket = socketSelector(baseComponent);

            // Fetch price-range candidates from DB, then filter by socket in memory
            var candidates = await _context.Set<TEntity>()
                .Where(e => e.Id != id && e.AveragePrice >= minPrice && e.AveragePrice <= maxPrice)
                .ToListAsync(ct);

            var result = candidates
                .Where(e => string.IsNullOrEmpty(baseSocket) || socketSelector(e) == baseSocket)
                .OrderBy(e => Math.Abs((e.AveragePrice ?? 0) - basePrice))
                .Take(count)
                .ToList();

            return Result<List<object>>.Success(_mapper.Map<List<TDto>>(result).Cast<object>().ToList());
        }
    }
}
