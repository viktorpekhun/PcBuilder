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
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilder.SharedKernel.Persistence;
using Components.Application.Queries;

namespace Components.Application.Handlers
{
    public class GetComponentByIdHandler : IRequestHandler<GetComponentByIdQuery, object>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetComponentByIdHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<object> Handle(GetComponentByIdQuery request, CancellationToken cancellationToken)
        {
            object entity = request.ComponentType switch
            {
                ComponentType.Cpu => await _context.Set<Cpu>().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken),
                ComponentType.Gpu => await _context.Set<Gpu>().Include(g => g.GpuPowerConnectors).FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken),
                ComponentType.Motherboard => await _context.Set<Motherboard>()
                    .Include(m => m.CpuPowerConnectors).Include(m => m.PcleSlots).Include(m => m.M2Slots)
                    .Include(m => m.RearPorts).Include(m => m.InnerPorts)
                    .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken),
                ComponentType.Ram => await _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken),
                ComponentType.CpuCooler => await _context.Set<CpuCooler>().Include(c => c.CpuCoolerSockets).FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken),
                ComponentType.PcCase => await _context.Set<PcCase>()
                    .Include(c => c.PcCaseFormFactors).Include(c => c.PcCaseFanLocations)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken),
                ComponentType.PowerSupply => await _context.Set<PowerSupply>().Include(p => p.PowerSupplyPowerConnectors).FirstOrDefaultAsync(ps => ps.Id == request.Id, cancellationToken),
                ComponentType.Ssd => await _context.Set<Ssd>().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken),
                ComponentType.Hdd => await _context.Set<Hdd>().FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken),
                ComponentType.Fan => await _context.Set<Fan>().FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken),
                _ => throw new ArgumentException("Unsupported component type")
            };

            if (entity == null)
                throw new NotFoundException($"Component with ID {request.Id} not found for type {request.ComponentType}");

            var offers = await _context.Set<ProductOffer>()
                .Include(p => p.Store)
                .Where(p => p.ComponentId == request.Id && p.ComponentType == request.ComponentType)
                .ToListAsync(cancellationToken);

            object dto = request.ComponentType switch
            {
                ComponentType.Cpu => _mapper.Map<CpuDto>(entity),
                ComponentType.Gpu => _mapper.Map<GpuDto>(entity),
                ComponentType.Motherboard => _mapper.Map<MotherboardDto>(entity),
                ComponentType.Ram => _mapper.Map<RamDto>(entity),
                ComponentType.CpuCooler => _mapper.Map<CpuCoolerDto>(entity),
                ComponentType.PcCase => _mapper.Map<PcCaseDto>(entity),
                ComponentType.PowerSupply => _mapper.Map<PowerSupplyDto>(entity),
                ComponentType.Ssd => _mapper.Map<SsdDto>(entity),
                ComponentType.Hdd => _mapper.Map<HddDto>(entity),
                ComponentType.Fan => _mapper.Map<FanDto>(entity),
                _ => throw new ArgumentException("Unsupported component type")
            };

            if (dto is IHasProductOffers dtoWithOffers)
                dtoWithOffers.ProductOffers = _mapper.Map<List<ProductOfferDto>>(offers);

            return dto;
        }
    }
}
