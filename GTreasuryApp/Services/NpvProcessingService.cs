using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Utilities.Dtos;
using GTreasury.Api.Utilities.Helpers;
using GTreasury.Api.Utilities.Records;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace GTreasury.Api.Functions.Services
{
    public class NpvProcessingService(
    INpvCalculator calculator,
    IFeatureManager featureManager,
    IOptions<NpvSettings> options,
    ILogger<NpvProcessingService> logger) : INpvProcessingService
    {
        private readonly NpvSettings _settings = options.Value;
        private readonly INpvCalculator _calculator = calculator;
        private readonly IFeatureManager _featureManager = featureManager;
        private readonly ILogger<NpvProcessingService> _logger = logger;

        //could have used strategy pattern

        public async Task<object> ProcessAsync(NpvInput input)
        {
            if (await _featureManager.IsEnabledAsync("EnableNpvBatchProcessing"))
            {
                _logger.LogInformation("Batch NPV processing enabled.");

                var rates = _calculator.GetRateRange(
                    input.LowerRate,
                    input.UpperRate,
                    input.Increment);

                var chunks = BatchHelper.Chunk(rates, size: _settings.MaxBatchSize);  //10

                return await Task.FromResult(chunks
                    .SelectMany(chunk => _calculator.CalculateBatch(input, chunk))
                    .ToList());
            }

            _logger.LogInformation("Single NPV processing enabled.");
            return _calculator.Calculate(input);
        }
    }
}
