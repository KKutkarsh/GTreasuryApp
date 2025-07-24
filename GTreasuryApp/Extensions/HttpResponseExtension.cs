using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace GTreasury.Api.Functions.Extensions
{
    public static class HttpResponseExtension
    {
        public static async Task<HttpResponseData> CreateJsonResponseAsync<T>(
       this HttpRequestData req,
       HttpStatusCode statusCode,
       T body)
        {
            var res = req.CreateResponse(statusCode);
            await res.WriteAsJsonAsync(body);
            return res;
        }
    }
}
