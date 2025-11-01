using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexResponse<T>
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("list")]
        public T? List { get; set; }
    }
}