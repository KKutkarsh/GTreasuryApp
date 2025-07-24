using GTreasury.Api.Functions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace GTreasury.Api.Functions.Https;

public class Test
{
    private readonly ILogger<Test> _logger;

    public Test(ILogger<Test> logger)
    {
        _logger = logger;
    }

    [Function("Test")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
       return await req.CreateJsonResponseAsync(HttpStatusCode.OK, "result");
    }
}