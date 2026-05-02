using MediatR;
using PcBuilder.SharedKernel;
using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.Commands;

namespace PcBuilds.Application.Handlers
{
    public class AutoBuildCommandHandler : IRequestHandler<AutoBuildCommand, Result<AutoBuildResultDto>>
    {
        private readonly IAutoBuilderService _autoBuilderService;

        public AutoBuildCommandHandler(IAutoBuilderService autoBuilderService)
        {
            _autoBuilderService = autoBuilderService;
        }

        public Task<Result<AutoBuildResultDto>> Handle(AutoBuildCommand request, CancellationToken cancellationToken)
            => _autoBuilderService.BuildAsync(request.Request, cancellationToken);
    }
}
