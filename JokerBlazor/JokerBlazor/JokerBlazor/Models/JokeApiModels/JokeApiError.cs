using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JokerBlazor.Components.Pages
{
    public partial class Jokes
    {
        public class JokeApiError
        {
            [JsonPropertyName("error")]
            [Required] public bool Error { get; set; }

            [JsonPropertyName("internalError")]
            public bool InternalError { get; set; }

            [JsonPropertyName("code")]
            public int Code { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("causedBy")]
            public string[]? CausedBy { get; set; }

            [JsonPropertyName("additionalInfo")]
            public string? AdditionalInfo { get; set; }

            [JsonPropertyName("timeStamp")]
            public long TimeStamp { get; set; }

            public void LogJokeApiErrorDetails()
            {
                Log.Information("Error Error: {error}", Error);
                Log.Information("Error InternalError: {internalError}", InternalError);
                Log.Information("Error Code: {code}", Code);
                Log.Information("Error Message: {message}", Message);
                Log.Information("Error CausedBy: {causedBy}", string.Join(", ", CausedBy!));
                Log.Information("Error AdditionalInfo: {additionalInfo}", AdditionalInfo);
                Log.Information("Error TimeStamp: {timeStamp}", TimeStamp);
            }
        }
    }
}