using Serilog;
using System.Text.Json.Serialization;

namespace JokerBlazor.Components.Pages
{
    public partial class Jokes
    {
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
    }
}