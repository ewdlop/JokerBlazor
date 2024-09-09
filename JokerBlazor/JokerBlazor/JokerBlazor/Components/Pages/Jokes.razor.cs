using Serilog;
using System.Text.Json;

namespace JokerBlazor.Components.Pages
{
    public partial class Jokes
    {
        private bool IsLoading;
        private string DisplayUrl = string.Empty;
        private string? JokeText;

        private readonly string BaseRequestUrl = "https://sv443.net/jokeapi/v2/joke/Any";
        private readonly string[] BlacklistFlags = ["nsfw", "religious", "political", "racist", "sexist", "explicit"];
        private readonly Dictionary<string, bool> SelectedFlags = [];

        protected override void OnInitialized()
        {
            foreach (var flag in BlacklistFlags)
            {
                SelectedFlags[flag] = false;
            }

            DisplayUrl = BaseRequestUrl;
        }

        private async Task GetJoke()
        {
            IsLoading = true;
            JokeText = string.Empty;
            Log.Information("Requesting new Joke.");

            try
            {
                string _requestUrl = BuildRequestUrl();
                Log.Information("RequestUrl: {requestUrl}", _requestUrl);

                Log.Information("Updating DisplayUrl.");
                DisplayUrl = _requestUrl;
                using HttpClient client = new();
                using HttpResponseMessage response = await client.GetAsync(_requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    Log.Information("API Response: \r\n{content}", _content);
                    Log.Information("Parsing JSON Document.");
                    var _jsonDocument = JsonDocument.Parse(_content);
                    var _rootElement = _jsonDocument.RootElement;

                    // Check Error Property for which JSON Object the Response is.
                    if (_rootElement.TryGetProperty("error", out JsonElement errorElement) && errorElement.GetBoolean())
                    {
                        // JokeApiError JSON
                        Log.Warning("JokeApiError JSON Object Detected!");
                        Log.Information("Deserializing JokeApiError JSON response.");
                        var _error = JsonSerializer.Deserialize<JokeApiError>(_content);
                        if (_error != null)
                        {
                            // Log Error JSON Details
                            _error.LogJokeApiErrorDetails();

                            // Update Joke Text
                            Log.Information("Updating Joke Text with Error Message.");
                            JokeText = $"Error: {_error.Message}";
                        }
                        else
                        {
                            Log.Error("Failed to deserialize JokeApiError JSON.\r\ncontent.ToString() {content}", _content.ToString());
                        }
                    }
                    else
                    {
                        // Joke JSON
                        Log.Information("Joke JSON Object Detected!");
                        Log.Information("Deserializing Joke JSON response.");
                        var _joke = JsonSerializer.Deserialize<Joke>(_content);
                        if (_joke != null)
                        {
                            // Log Joke JSON Details
                            _joke.LogJokeDetails();

                            // Update Joke Text
                            Log.Information("Updating Joke Text.");
                            JokeText = $"{_joke.Setup} \r\n {_joke.Delivery}";
                        }
                        else
                        {
                            Log.Error("Failed to deserialize Joke JSON.\r\ncontent.ToString() {content}", _content.ToString());
                        }
                    }
                }
                else
                {
                    JokeText = $"Unsuccessful StatusCode.\r\nStatusCode: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting Joke.");
            }

            IsLoading = false;
        }

        private void GetPreviousJoke()
        {
            try
            {
                Log.Information("Getting Previous Joke.");
                string? previousJoke = JokeText;
                Log.Information("Previous Joke: {previousJoke}", previousJoke);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting Previous Joke.");
            }
        }

        private string BuildRequestUrl()
        {
            try
            {
                Log.Information("Building API Request URL...");
                var _selectedFlagsList = SelectedFlags.Where(f => f.Value).Select(f => f.Key).ToList();

                if (_selectedFlagsList.Count > 0)
                {
                    Log.Information("Flags selected from the UI: {selectedFlags}", string.Join(", ", _selectedFlagsList));
                    Log.Information("Adding BlacklistFLags to Url");
                    var blackListUrl = $"?blacklistFlags={string.Join(",", _selectedFlagsList)}";
                    return $"{BaseRequestUrl}{blackListUrl}";
                }
                else
                {
                    Log.Information("No BlacklistFlags selected. Using base request Url.");
                    return BaseRequestUrl;
                }
            }
            catch (Exception _ex)
            {
                Log.Error(_ex, "An error occurred in BuildRequestUri");
                return string.Empty;
            }
        }
    }
}