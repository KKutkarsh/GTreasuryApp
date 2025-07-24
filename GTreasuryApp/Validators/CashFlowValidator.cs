using FluentValidation;
using GTreasury.Api.Utilities.Records;

namespace GTreasury.Api.Functions.Validators
{
    internal class CashFlowValidator : AbstractValidator<CashFlow>
    {
        public CashFlowValidator()
        {
            RuleFor(x => x.Year)
            .NotNull().WithMessage("year cannot be null")
            .GreaterThanOrEqualTo(1900).WithMessage("Year must be no earlier than 1900.")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1000).WithMessage("Year must be within a reasonable range.");

            RuleFor(x => x.Amount)
                .NotNull().WithMessage($"Amount Cannot be null")
                .NotEqual(0).WithMessage("Amount Can not be Zero");
        }
    }
}
