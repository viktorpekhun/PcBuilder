using FluentValidation;
using PcBuilds.Application.Commands;

namespace PcBuilds.Application.Validators
{
    public class UpdateBuildCommandValidator : AbstractValidator<UpdateBuildCommand>
    {
        public UpdateBuildCommandValidator(PcBuildInputDtoValidator buildDtoValidator)
        {
            RuleFor(x => x.PcBuildId)
                .NotEmpty().WithMessage("Build ID is required.");

            RuleFor(x => x.BuildDto)
                .NotNull().WithMessage("Build data is required.")
                .SetValidator(buildDtoValidator);
        }
    }
}