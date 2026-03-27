using FluentValidation;
using PcBuilds.Application.Commands;

namespace PcBuilds.Application.Validators
{
    public class SaveBuildCommandValidator : AbstractValidator<SaveBuildCommand>
    {
        public SaveBuildCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.BuildDto)
                .NotNull().WithMessage("Build data is required.")
                .SetValidator(new PcBuildInputDtoValidator());
        }
    }
}
