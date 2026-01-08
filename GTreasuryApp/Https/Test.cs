using GTreasury.Api.Functions.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System.Net;

namespace GTreasury.Api.Functions.Https;

public class Test(IFeatureManager featureManager, ILogger<Test> logger)
{
    private readonly IFeatureManager _featureManager = featureManager;
    private readonly ILogger<Test> _logger = logger;


    [Function(nameof(Test))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        if (await _featureManager.IsEnabledAsync("EnableNpvBatchProcessing"))
        {
            _logger.LogInformation("EnableNpvBatchProcessing is enabled.");
        }
        else
        {
            _logger.LogInformation("EnableNpvBatchProcessing is disabled.");
        }
        return await req.CreateJsonResponseAsync(HttpStatusCode.OK, "result");
    }
}