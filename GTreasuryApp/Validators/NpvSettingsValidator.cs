using FluentValidation;
using GTreasury.Api.Utilities.Dtos;

namespace GTreasury.Api.Functions.Validators
{
    public class NpvSettingsValidator : AbstractValidator<NpvSettings>
    {
        public NpvSettingsValidator()
        {
            RuleFor(x => x.MaxBatchSize).NotEmpty()
                .InclusiveBetween(1, 100);
        }
    }
}
