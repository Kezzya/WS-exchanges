using Newtonsoft.Json;

namespace ChainApex.Net.Objects.Models.Socket
{
    public class ChainApexSocketRequest
    {
        [JsonProperty("event")]
        public string Event { get; set; } = string.Empty;

        [JsonProperty("params")]
        public ChainApexSocketRequestParams Params { get; set; } = new();
    }

    public class ChainApexSocketRequestParams
    {
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonProperty("cb_id")]
        public string? CbId { get; set; }

        [JsonProperty("endIdx", NullValueHandling = NullValueHandling.Ignore)]
        public string? EndIdx { get; set; }

        [JsonProperty("pageSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? PageSize { get; set; }
    }
}

