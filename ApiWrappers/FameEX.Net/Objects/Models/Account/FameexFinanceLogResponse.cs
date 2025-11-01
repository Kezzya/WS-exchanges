using Newtonsoft.Json;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexFinanceLogResponse
    {
        [JsonProperty("finance")]
        public List<FameexFinanceLog> Finance { get; set; } = new();

        [JsonProperty("total")]
        public string Total { get; set; } = string.Empty;
    }
}