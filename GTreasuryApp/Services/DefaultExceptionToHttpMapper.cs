using FluentValidation;
using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Utilities.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace GTreasury.Api.Functions.Services
{
    public class DefaultExceptionToHttpMapper : IDefaultExceptionToHttpMapper
    {
        public HttpStatusCode Map(Exception exception)
        {
            return exception switch
            {
                ArgumentException or ArgumentNullException or BadHttpRequestException or BadRequestException => HttpStatusCode.BadRequest,
                ValidationException => HttpStatusCode.BadRequest,
                KeyNotFoundException or FileNotFoundException or NotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                NotImplementedException => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}
