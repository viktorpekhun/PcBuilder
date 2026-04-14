using FluentValidation;
using Moderation.Application.Commands;

namespace Moderation.Application.Validators
{
    public class ReportReviewCommandValidator : AbstractValidator<ReportReviewCommand>
    {
        public ReportReviewCommandValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty().WithMessage("Review ID is required.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
