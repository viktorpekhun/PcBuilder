using FluentValidation;
using PcBuilder.SharedKernel.Services;
using PcBuilds.Application.Commands;

namespace PcBuilds.Application.Validators
{
    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator(IProfanityFilterService profanityFilter)
        {
            RuleFor(x => x.PcBuildId).NotEmpty().WithMessage("Build ID is required.");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
            RuleFor(x => x.Rating)
                .InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5.");
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Comment text is required.")
                .MaximumLength(500).WithMessage("Comment can't exceed 500 characters.")
                .Must(text => !profanityFilter.ContainsProfanity(text))
                .WithMessage("Comment contains inappropriate language.");
        }
    }
}