using Auth.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record RegisterCommand(string Username, string Email, string Password)
        : IRequest<Result<AuthResultDto>>;
}
