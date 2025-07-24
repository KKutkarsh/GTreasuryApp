using FluentValidation;
using GTreasury.Api.Functions.Middlewares;
using GTreasury.Api.Functions.Services;
using GTreasury.Api.Functions.Services.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args); // Changed to use Host.CreateDefaultBuilder

builder.ConfigureFunctionsWebApplication(workerApp =>
{
    workerApp.UseMiddleware<ErrorHandlerMiddleware>();
});

builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

    services.AddSingleton<IDefaultExceptionToHttpMapper, DefaultExceptionToHttpMapper>();
    services.AddSingleton<IDefaultErrorResponseBuilder, DefaultErrorResponseBuilder>();
    services.AddScoped<INpvCalculator, NpvCalculator>();
    services.AddValidatorsFromAssemblyContaining<Program>();
});

var host = builder.Build();
host.Run();
