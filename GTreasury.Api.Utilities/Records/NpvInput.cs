using System.Text.Json.Serialization;

namespace GTreasury.Api.Utilities.Records
{
    public record NpvInput
    {
        //[JsonPropertyName("cash_flows")]
        public List<CashFlow> CashFlows { get; init; } = [];

        //[JsonPropertyName("lower_rate")]
        public double LowerRate { get; init; }

        //[JsonPropertyName("upper_rate")]
        public double UpperRate { get; init; }

        [JsonPropertyName("increment")]
        public double Increment { get; init; }

        public int EvaluationYear { get; set; }
    }
}
