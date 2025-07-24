namespace GTreasury.Api.Utilities.Records
{
    public record CashFlow
    {
        public int Year { get; init; }
        public double Amount { get; init; }
    }
}
