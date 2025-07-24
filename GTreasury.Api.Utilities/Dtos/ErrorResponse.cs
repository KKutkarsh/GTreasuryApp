using System.Text.Json.Serialization;

namespace GTreasury.Api.Utilities.Dtos
{
    public class ErrorResponse
    {
        public string Type { get; set; }
        public string Message { get; set; }
        [JsonIgnore]
        public string? StackTrace { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
        [JsonIgnore]
        public string? InnerException { get; set; }

        public ErrorResponse(Exception ex)
        {
            Type = ex.GetType().Name;
            Message = ex.Message;
            StackTrace = ex.StackTrace;

            if (ex.InnerException != null)
            {
                InnerException = $"{ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
            }
        }
    }
}
