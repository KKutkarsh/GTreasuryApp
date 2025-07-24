using GTreasury.Api.Functions.Extensions;
using GTreasury.Api.Functions.Services.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace GTreasury.Api.Functions.Middlewares
{
    public class ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> _logger,
        IDefaultExceptionToHttpMapper defaultExceptionToHttpMapper,
        IDefaultErrorResponseBuilder defaultErrorResponseBuilder) : IFunctionsWorkerMiddleware
    {
        private readonly IDefaultExceptionToHttpMapper _defaultExceptionToHttpMapper = defaultExceptionToHttpMapper;
        private readonly IDefaultErrorResponseBuilder _defaultErrorResponseBuilder = defaultErrorResponseBuilder;

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(FunctionContext context, Exception ex)
        {

            var requestData = await context.GetHttpRequestDataAsync();

            _logger.LogError(ex,
            "Unhandled exception occurred in function '{FunctionName}' at path '{Path}'. Exception: {ExceptionMessage}",
            context.FunctionDefinition.Name,
            requestData?.Url.AbsolutePath ?? "N/A",
            ex.Message);

            var statusCode = _defaultExceptionToHttpMapper.Map(ex);
            var errorResponse = _defaultErrorResponseBuilder.Build(ex);

            await requestData.CreateJsonResponseAsync(statusCode, errorResponse);
        }
    }
}
