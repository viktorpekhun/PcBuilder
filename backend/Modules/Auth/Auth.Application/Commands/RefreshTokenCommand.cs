using Auth.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record RefreshTokenCommand(string? RefreshToken)
        : IRequest<Result<AuthResultDto>>;
}
