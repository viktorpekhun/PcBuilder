using FluentValidation;
using Moderation.Application.Commands;

namespace Moderation.Application.Validators
{
    public class AdminDeleteReviewCommandValidator : AbstractValidator<AdminDeleteReviewCommand>
    {
        public AdminDeleteReviewCommandValidator()
        {
            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required.");

            RuleFor(x => x.ReviewId)
                .NotEmpty().WithMessage("Review ID is required.");
        }
    }
}
