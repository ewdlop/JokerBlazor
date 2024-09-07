using Serilog;
using System.Text.Json;

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
    }
}