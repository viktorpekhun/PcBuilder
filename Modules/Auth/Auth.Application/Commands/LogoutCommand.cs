using MediatR;

namespace Auth.Application.Commands
{
    public record LogoutCommand(string? RefreshToken) : IRequest;
}
