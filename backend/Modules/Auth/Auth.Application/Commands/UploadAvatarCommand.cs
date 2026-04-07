using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record UploadAvatarCommand(Guid UserId, byte[] Data, string Extension, string BaseUrl) : IRequest<Result<string>>;
}
