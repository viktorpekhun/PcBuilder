using FluentValidation;
using Moderation.Application.Commands;

namespace Moderation.Application.Validators
{
    public class IssueBanCommandValidator : AbstractValidator<IssueBanCommand>
    {
        public IssueBanCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.BanType)
                .IsInEnum().WithMessage("Invalid ban type.");

            RuleFor(x => x.DurationDays)
                .GreaterThan(0).WithMessage("Ban duration must be at least 1 day.")
                .LessThanOrEqualTo(365).WithMessage("Ban duration cannot exceed 365 days.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
