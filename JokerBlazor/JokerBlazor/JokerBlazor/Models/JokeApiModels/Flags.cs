using System.Text.Json.Serialization;

namespace JokerBlazor.Components.Pages
{
    public partial class Jokes
    {
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
    }
}