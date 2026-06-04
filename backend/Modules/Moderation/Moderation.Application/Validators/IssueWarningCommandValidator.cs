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

            RuleFor(x => x.ReasonCode)
                .NotEmpty().WithMessage("Reason code is required.")
                .Must(WarnReasonCodes.All.Contains).WithMessage("Invalid reason code.");
        }
    }
}
