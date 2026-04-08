using Auth.Application.Commands;
using FluentValidation;

namespace Auth.Application.Validators
{
    public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Google ID token is required.");
        }
    }
}
