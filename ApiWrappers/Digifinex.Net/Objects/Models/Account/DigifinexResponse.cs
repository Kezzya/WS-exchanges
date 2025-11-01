// DigifinexRestClientSpotApiAccount.cs

using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account
{
    // Enums

    // Models
    public class DigifinexResponse<T>
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("list")]
        public T? List { get; set; }
    }
}