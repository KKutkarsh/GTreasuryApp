using FluentValidation;
using GTreasury.Api.Functions.Extensions;
using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Functions.Validators;
using GTreasury.Api.Utilities;
using GTreasury.Api.Utilities.Exceptions;
using GTreasury.Api.Utilities.Helpers;
using GTreasury.Api.Utilities.Records;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GTreasury.Api.Functions.Https;

/// <summary>
/// you can use MSAL authentication and authorization with Azure AD
/// </summary>
/// <param name="validator"></param>
/// <param name="logger"></param>

public class PostNpvValues(INpvCalculator npvCalculator, NpvInputValidator validator, ILogger<PostNpvValues> logger)
{
    private readonly ILogger<PostNpvValues> _logger = logger;
    private readonly NpvInputValidator _validtor = validator;
    private readonly INpvCalculator _npvCalculator = npvCalculator;

    [Function(nameof(PostNpvValues))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = AppConstants.RouteConstants.PostNpvValuesUrl)] HttpRequestData req)
    {
        var input = await req.DeserializeRequestBodyAsync<NpvInput>();

        var validationResult = _validtor.Validate(input);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        } 

        var result = _npvCalculator.Calculate(input);


        //For batch Processing

        //var rateList = _npvCalculator.GetRateRange(input.LowerRate, input.UpperRate, input.Increment);
        //var rateChunks = BatchHelper.Chunk(rateList, size: 10);

        //var result1 = rateChunks.SelectMany(batchOfRates => _npvCalculator.CalculateBatch(input, batchOfRates)).OrderBy(r => r.Rate).ToList();

        return await req.CreateJsonResponseAsync(System.Net.HttpStatusCode.OK, result);
    }
}