using FluentValidation;
using GTreasury.Api.Utilities.Records;

namespace GTreasury.Api.Functions.Validators
{
    public class NpvInputValidator : AbstractValidator<NpvInput>
    {
        public NpvInputValidator()
        {
            RuleFor(x => x.LowerRate).GreaterThan(0);
            RuleFor(x => x.UpperRate).GreaterThan(x => x.LowerRate);
            RuleFor(x => x.Increment).GreaterThan(0);

            RuleFor(x => x.CashFlows)
                .NotEmpty().WithMessage("At least one cash flow is required.")
                .ForEach(flow => flow.SetValidator(new CashFlowValidator()));
        }
    }
}
