using FluentValidation;
using Moderation.Application.Commands;

namespace Moderation.Application.Validators
{
    public class ReportBuildCommandValidator : AbstractValidator<ReportBuildCommand>
    {
        public ReportBuildCommandValidator()
        {
            RuleFor(x => x.BuildId)
                .NotEmpty().WithMessage("Build ID is required.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
