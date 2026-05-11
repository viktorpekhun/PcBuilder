using FluentValidation;
using Moderation.Application.Commands;

namespace Moderation.Application.Validators
{
    public class IssueWarningCommandValidator : AbstractValidator<IssueWarningCommand>
    {
        public IssueWarningCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.BanType)
                .IsInEnum().WithMessage("Invalid ban type.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
