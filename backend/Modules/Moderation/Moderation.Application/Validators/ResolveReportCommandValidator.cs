using FluentValidation;
using Moderation.Application.Commands;
using Moderation.Domain.Enums;

namespace Moderation.Application.Validators
{
    public class ResolveReportCommandValidator : AbstractValidator<ResolveReportCommand>
    {
        public ResolveReportCommandValidator()
        {
            RuleFor(x => x.ReportId)
                .NotEmpty().WithMessage("Report ID is required.");

            RuleFor(x => x.Action)
                .IsInEnum().WithMessage("Invalid resolution action.");

            RuleFor(x => x.BanDurationDays)
                .GreaterThan(0).WithMessage("Ban duration must be at least 1 day.")
                .LessThanOrEqualTo(365).WithMessage("Ban duration cannot exceed 365 days.")
                .When(x => x.Action == ReportResolutionAction.DeleteContentAndBan);

            RuleFor(x => x.BanType)
                .NotNull().WithMessage("Ban type is required for ban action.")
                .When(x => x.Action == ReportResolutionAction.DeleteContentAndBan);

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Reason));

            RuleFor(x => x.ReasonCode)
                .NotEmpty().WithMessage("Reason code is required.")
                .Must(code => WarnReasonCodes.All.Contains(code!)).WithMessage("Invalid reason code.")
                .When(x => x.Action == ReportResolutionAction.DeleteContentAndWarn);
        }
    }
}
