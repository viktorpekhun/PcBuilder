using FluentValidation;
using PcBuilds.Application.Commands;

namespace PcBuilds.Application.Validators
{
    public class CloneBuildCommandValidator : AbstractValidator<CloneBuildCommand>
    {
        public CloneBuildCommandValidator()
        {
            RuleFor(x => x.SourceBuildId).NotEmpty().WithMessage("Source build ID is required.");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}