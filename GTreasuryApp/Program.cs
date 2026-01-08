using FluentValidation;
using GTreasury.Api.Functions.Middlewares;
using GTreasury.Api.Functions.Services;
using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Functions.Validators;
using GTreasury.Api.Utilities.Dtos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
});

builder.ConfigureFunctionsWebApplication(workerApp =>
{
    workerApp.UseMiddleware<ErrorHandlerMiddleware>();
});

builder.ConfigureServices((context,services) =>
{

    //Can use Options custom validation and ValidationOnStart aswell in .net10

    var npvSettings = context.Configuration
        .GetSection("NpvSettings")
        .Get<NpvSettings>();

    var validator = new NpvSettingsValidator();
    var validationResult = validator.Validate(npvSettings);

    if (!validationResult.IsValid)
    {
        var errors = string.Join(
            Environment.NewLine,
            validationResult.Errors.Select(e => e.ErrorMessage));

        throw new InvalidOperationException(
            $"Invalid NpvSettings configuration:{Environment.NewLine}{errors}");
    }

    // 3️⃣ Register the validated settings
    services.AddSingleton(npvSettings);

    services.AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

    services.AddFeatureManagement();

    services.AddSingleton<IDefaultExceptionToHttpMapper, DefaultExceptionToHttpMapper>();
    services.AddSingleton<IDefaultErrorResponseBuilder, DefaultErrorResponseBuilder>();
    services.AddScoped<INpvProcessingService, NpvProcessingService>();
    services.AddScoped<INpvCalculator, NpvCalculator>();
    services.AddValidatorsFromAssemblyContaining<Program>();
});

var host = builder.Build();
host.Run();
