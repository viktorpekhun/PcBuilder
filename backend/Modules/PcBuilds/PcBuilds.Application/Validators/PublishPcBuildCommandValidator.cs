using FluentValidation;
using PcBuilds.Application.Commands;

namespace PcBuilds.Application.Validators
{
    public class PublishPcBuildCommandValidator : AbstractValidator<PublishPcBuildCommand>
    {
        public PublishPcBuildCommandValidator()
        {
            RuleFor(x => x.PcBuildId).NotEmpty().WithMessage("Build ID is required.");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}