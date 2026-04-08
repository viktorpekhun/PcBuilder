using Auth.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Queries
{
    public record GetProfileQuery(Guid UserId) : IRequest<Result<ProfileDto>>;
}
