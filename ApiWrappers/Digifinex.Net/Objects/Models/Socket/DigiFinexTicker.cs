using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Socket
{
    public class DigifinexTicker
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("open_24h")]
        public decimal? Open24h { get; set; }

        [JsonProperty("low_24h")]
        public decimal? Low24h { get; set; }

        [JsonProperty("base_volume_24h")]
        public decimal? BaseVolume24h { get; set; }

        [JsonProperty("quote_volume_24h")]
        public decimal? QuoteVolume24h { get; set; }

        [JsonProperty("last")]
        public decimal? Last { get; set; }

        [JsonProperty("last_qty")]
        public decimal? LastQty { get; set; }

        [JsonProperty("best_bid")]
        public decimal? BestBid { get; set; }

        [JsonProperty("best_bid_size")]
        public decimal? BestBidSize { get; set; }

        [JsonProperty("best_ask")]
        public decimal? BestAsk { get; set; }

        [JsonProperty("best_ask_size")]
        public decimal? BestAskSize { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}