using AutoMapper;
using Components.Application.Dtos;
using Components.Application.Queries;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class GetPriceAlertForComponentHandler : IRequestHandler<GetPriceAlertForComponentQuery, Result<PriceAlertDto?>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetPriceAlertForComponentHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PriceAlertDto?>> Handle(GetPriceAlertForComponentQuery request, CancellationToken cancellationToken)
        {
            var subscription = await _context.Set<PriceAlertSubscription>()
                .FirstOrDefaultAsync(
                    s => s.UserId == request.UserId
                         && s.ComponentId == request.ComponentId
                         && s.ComponentType == request.ComponentType,
                    cancellationToken);

            var dto = subscription is null ? null : _mapper.Map<PriceAlertDto>(subscription);
            return Result.Success(dto);
        }
    }
}
