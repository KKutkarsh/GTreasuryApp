using GTreasury.Api.Utilities.Records;

namespace GTreasury.Api.Functions.Services.Interface
{
    public interface INpvProcessingService
    {
            Task<object> ProcessAsync(NpvInput input);
    }
}
