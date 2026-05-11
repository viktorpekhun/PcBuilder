using MediatR;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record ReportBuildCommand(Guid ReporterId, Guid BuildId, string Reason) : IRequest<Result<Guid>>;
}