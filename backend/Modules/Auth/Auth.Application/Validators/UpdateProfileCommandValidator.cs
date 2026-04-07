using Auth.Application.Commands;
using FluentValidation;

namespace Auth.Application.Validators
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(30).WithMessage("Username can't exceed 30 characters.")
                .Matches(@"^[a-zA-Z0-9_-]+$")
                .WithMessage("Username can only contain letters, numbers, underscores and hyphens.");

            RuleFor(x => x.Bio)
                .MaximumLength(200).WithMessage("Bio can't exceed 200 characters.")
                .When(x => x.Bio != null);
        }
    }
}
