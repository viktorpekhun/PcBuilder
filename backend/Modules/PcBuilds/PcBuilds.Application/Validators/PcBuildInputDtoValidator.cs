using FluentValidation;
using PcBuilder.SharedKernel.Services;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Validators
{
    public class PcBuildInputDtoValidator : AbstractValidator<PcBuildInputDto>
    {
        public PcBuildInputDtoValidator(IProfanityFilterService profanityFilter)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name can't have more than 255 characters.")
                .Matches(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐёЁ0-9 _-]*$")
                .WithMessage("Name can contain letters, numbers, spaces, underscores (_) and hyphens (-)")
                .Must(name => !profanityFilter.ContainsProfanity(name))
                .WithMessage("Build name contains inappropriate language.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description can't have more than 500 characters.")
                .Must(desc => desc == null || !profanityFilter.ContainsProfanity(desc))
                .WithMessage("Description contains inappropriate language.");

            RuleForEach(x => x.Rams).SetValidator(new ComponentQuantityDtoValidator());
            RuleForEach(x => x.Ssds).SetValidator(new ComponentQuantityDtoValidator());
            RuleForEach(x => x.Hdds).SetValidator(new ComponentQuantityDtoValidator());
            RuleForEach(x => x.Fans).SetValidator(new ComponentQuantityDtoValidator());
        }
    }

    public class ComponentQuantityDtoValidator : AbstractValidator<ComponentQuantityDto>
    {
        public ComponentQuantityDtoValidator()
        {
            RuleFor(x => x.ComponentId)
                .NotEmpty().WithMessage("Component ID is required.");

            RuleFor(x => x.Quantity)
                .InclusiveBetween(1, 10).WithMessage("Quantity must be between 1 and 10.");
        }
    }
}