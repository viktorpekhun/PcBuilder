using MediatR;
using PcBuilder.SharedKernel;

namespace PcBuilds.Application.Commands
{
    public record UploadBuildPhotoCommand(Guid BuildId, Guid UserId, byte[] Data) : IRequest<Result<string>>;
}
