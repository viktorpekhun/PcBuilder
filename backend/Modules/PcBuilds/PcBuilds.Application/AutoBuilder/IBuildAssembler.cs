using PcBuilds.Application.AutoBuilder.Models;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.AutoBuilder
{
    public interface IBuildAssembler
    {
        Task<PcBuild?> TryAssembleAsync(CorePairing core, AssemblyContext ctx, CancellationToken ct);
    }
}
