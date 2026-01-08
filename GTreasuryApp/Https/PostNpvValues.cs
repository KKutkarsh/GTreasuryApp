using FluentValidation;
using GTreasury.Api.Functions.Extensions;
using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Functions.Validators;
using GTreasury.Api.Utilities;
using GTreasury.Api.Utilities.Records;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GTreasury.Api.Functions.Https;

/// <summary>
/// you can use MSAL authentication and authorization with Azure AD
/// More clean approach folloing solid principle
/// </summary>
/// <param name="validator"></param>
/// <param name="logger"></param>

public class PostNpvValues(INpvProcessingService processingService, NpvInputValidator validator, ILogger<PostNpvValues> logger)
{
    private readonly INpvProcessingService _processingService = processingService;
    private readonly NpvInputValidator _validator = validator;
    private readonly ILogger<PostNpvValues> _logger = logger;

    [Function(nameof(PostNpvValues))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = AppConstants.RouteConstants.PostNpvValuesUrl)] HttpRequestData req)
    {
        var input = await req.DeserializeRequestBodyAsync<NpvInput>();

        var validationResult = _validator.Validate(input);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        } 


        //var result1 = rateChunks.SelectMany(batchOfRates => _npvCalculator.CalculateBatch(input, batchOfRates)).OrderBy(r => r.Rate).ToList();

        return await req.CreateJsonResponseAsync(System.Net.HttpStatusCode.OK, result);
    }
}