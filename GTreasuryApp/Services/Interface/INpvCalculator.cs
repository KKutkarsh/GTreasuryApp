using GTreasury.Api.Utilities.Dtos;
using GTreasury.Api.Utilities.Records;

namespace GTreasury.Api.Functions.Services.Interface
{
    public interface INpvCalculator
    {
        IEnumerable<NpvResult> Calculate(NpvInput input);
        IEnumerable<NpvResult> CalculateParallel(NpvInput input);
        IEnumerable<NpvResult> CalculateBatch(NpvInput input, IEnumerable<double> rates);


        IEnumerable<double> GetRateRange(double lower, double upper, double increment);
    }
}
