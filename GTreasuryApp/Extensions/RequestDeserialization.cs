using GTreasury.Api.Utilities.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace GTreasury.Api.Functions.Extensions
{
    public static class RequestDeserialization
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<T> DeserializeRequestBodyAsync<T>(this HttpRequestData req)
        {

            if (req.Body == null)
            {
                throw new BadRequestException("Request body is required");
            }

            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(req.Body, serializerOptions);
                return result ?? throw new BadRequestException("Deserialized value cannot be null");
            }
            catch (JsonException)
            {
                throw;
            }
        }
    }
}
