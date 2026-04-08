using Auth.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record UpdateProfileCommand(Guid UserId, string Username, string? Bio) : IRequest<Result<ProfileDto>>;
}
