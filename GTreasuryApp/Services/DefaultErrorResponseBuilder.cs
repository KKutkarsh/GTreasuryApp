using FluentValidation;
using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Utilities;
using GTreasury.Api.Utilities.Dtos;

namespace GTreasury.Api.Functions.Services
{
    public class DefaultErrorResponseBuilder : IDefaultErrorResponseBuilder
    {
        public ErrorResponse Build(Exception exception)
        {
            var response = new ErrorResponse(exception);

            if (exception is ValidationException validationException)
            {
                response.Message = AppConstants.ExceptionMessages.ValidationFailure;
                response.Details["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
            }

            return response;
        }
    }
}
