using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JokerBlazor.Components.Pages
{
    public partial class Jokes
    {
        private bool _isLoading;
        private readonly string BaseRequestUrl = "https://sv443.net/jokeapi/v2/joke/Any";
        private string _displayUrl = string.Empty;

        private string? jokeText;

        private readonly string[] BlacklistFlags = ["nsfw", "religious", "political", "racist", "sexist", "explicit"];
        private readonly Dictionary<string, bool> SelectedFlags = [];

        protected override void OnInitialized()
        {
            foreach (var flag in BlacklistFlags)
            {
                SelectedFlags[flag] = false;
            }

            _displayUrl = BaseRequestUrl;
        }

        private async Task GetJoke()
        {
            _isLoading = true;
            Log.Information("Requesting new Joke.");

            try
            {
                string _requestUrl = BuildRequestUrl();
                Log.Information("RequestUrl: {requestUrl}", _requestUrl);

                Log.Information("Updating DisplayUrl.");
                _displayUrl = _requestUrl;
                using HttpClient client = new();
                using HttpResponseMessage response = await client.GetAsync(_requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Log.Information("API Response: \r\n{content}", content);
                    Log.Information("Parsing JSON Document.");
                    var jsonDocument = JsonDocument.Parse(content);
                    var rootElement = jsonDocument.RootElement;

                    // Check Error Property for which JSON Object the Response is.
                    if (rootElement.TryGetProperty("error", out JsonElement errorElement) && errorElement.GetBoolean())
                    {
                        // JokeApiError JSON
                        Log.Warning("JokeApiError JSON Object Detected!");
                        Log.Information("Deserializing JokeApiError JSON response.");
                        var error = JsonSerializer.Deserialize<JokeApiError>(content);
                        if (error != null)
                        {
                            // Log Error JSON Details
                            error.LogJokeApiErrorDetails();

                            // Update Joke Text
                            Log.Information("Updating Joke Text with Error Message.");
                            jokeText = $"Error: {error.Message}";
                        }
                        else
                        {
                            Log.Error("Failed to deserialize JokeApiError JSON.\r\ncontent.ToString() {content}", content.ToString());
                        }
                    }
                    else
                    {
                        // Joke JSON
                        Log.Information("Joke JSON Object Detected!");
                        Log.Information("Deserializing Joke JSON response.");
                        var joke = JsonSerializer.Deserialize<Joke>(content);
                        if (joke != null)
                        {
                            // Log Joke JSON Details
                            joke.LogJokeDetails();

                            // Update Joke Text
                            Log.Information("Updating Joke Text.");
                            jokeText = $"{joke.Setup} \r\n {joke.Delivery}";
                        }
                        else
                        {
                            Log.Error("Failed to deserialize Joke JSON.\r\ncontent.ToString() {content}", content.ToString());
                        }
                    }
                }
                else
                {
                    jokeText = $"Unsuccessful StatusCode.\r\nStatusCode: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting Joke.");
            }

            _isLoading = false;
        }

        private string BuildRequestUrl()
        {
            try
            {
                Log.Information("Building API Request URL...");
                var selectedFlagsList = SelectedFlags.Where(f => f.Value).Select(f => f.Key).ToList();

                if (selectedFlagsList.Count > 0)
                {
                    Log.Information("Flags selected from the UI: {selectedFlags}", string.Join(", ", selectedFlagsList));
                    Log.Information("Adding BlacklistFLags to Url");
                    var blackListUrl = $"?blacklistFlags={string.Join(",", selectedFlagsList)}";
                    return $"{BaseRequestUrl}{blackListUrl}";
                }
                else
                {
                    Log.Information("No BlacklistFlags selected. Using base request Url.");
                    return BaseRequestUrl;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in BuildRequestUri");
                return string.Empty;
            }
        }

        public class Joke
        {
            [JsonPropertyName("error")]
            public bool Error { get; set; }

            [JsonPropertyName("category")]
            public string? Category { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("setup")]
            public string? Setup { get; set; }

            [JsonPropertyName("delivery")]
            public string? Delivery { get; set; }

            [JsonPropertyName("flags")]
            public Flags? Flags { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("safe")]
            public bool Safe { get; set; }

            [JsonPropertyName("lang")]
            public string? Lang { get; set; }

            public void LogJokeDetails()
            {
                Log.Information("Joke Error: {error}", Error);
                Log.Information("Joke Category: {category}", Category);
                Log.Information("Joke Type: {type}", Type);
                Log.Information("Joke Setup: {setup}", Setup);
                Log.Information("Joke Delivery: {delivery}", Delivery);
                Log.Information("Joke Flags: {flags}", Flags);
                Log.Information("Joke Id: {id}", Id);
                Log.Information("Joke Safe: {safe}", Safe);
                Log.Information("Joke Type: {type}", Type);
                Log.Information("Joke Lang: {lang}", Lang);
            }
        }

        public class Flags
        {
            [JsonPropertyName("nsfw")]
            public bool Nsfw { get; set; }

            [JsonPropertyName("religious")]
            public bool Religious { get; set; }

            [JsonPropertyName("political")]
            public bool Political { get; set; }

            [JsonPropertyName("racist")]
            public bool Racist { get; set; }

            [JsonPropertyName("sexist")]
            public bool Sexist { get; set; }

            [JsonPropertyName("explicit")]
            public bool Explicit { get; set; }

            public override string ToString()
            {
                return $"Nsfw: {Nsfw}, Religious: {Religious}, Political: {Political}, Racist: {Racist}, Sexist: {Sexist}, Explicit: {Explicit}";
            }
        }

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