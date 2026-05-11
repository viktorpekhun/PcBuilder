using FluentValidation;
using Moderation.Application.Commands;

namespace Moderation.Application.Validators
{
    public class AdminDeleteBuildCommandValidator : AbstractValidator<AdminDeleteBuildCommand>
    {
        public AdminDeleteBuildCommandValidator()
        {
            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required.");

            RuleFor(x => x.BuildId)
                .NotEmpty().WithMessage("Build ID is required.");
        }
    }
}
