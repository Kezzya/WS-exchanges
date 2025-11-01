using Newtonsoft.Json;

using System;

namespace BaseStockConnectorInterface.Models.Ticker
{
    public class OptionTickerModel
    {
        public decimal? BestAskAmount { get; set; }

        public decimal? BestAskPrice { get; set; }

        public decimal? BestBidAmount { get; set; }

        public decimal? BestBidPrice { get; set; }

        public decimal? CurrentFunding { get; set; }

        public decimal? EstimatedDeliveryPrice { get; set; }

        public decimal? Funding8h { get; set; }

        public decimal? IndexPrice { get; set; }

        public string InstrumentName { get; set; }

        public decimal? InterestValue { get; set; }

        public decimal? LastPrice { get; set; }

        public decimal? MarkPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MarkIv { get; set; }

        public decimal? AskIv { get; set; }

        public decimal? BidIv { get; set; }

        public decimal? UnderlyingPrice { get; set; }

        public string UnderlyingIndex { get; set; }

        public decimal? OpenInterest { get; set; }

        public decimal? SettlementPrice { get; set; }

        public decimal? InterestRate { get; set; }

        public string State { get; set; }

        public OptionGreeks Greeks { get; set; }

        public OptionTickerStats Stats { get; set; }

        public DateTime? Timestamp { get; set; }
    }

    public class OptionGreeks
    {
        [JsonProperty("delta")]
        public decimal? Delta { get; set; }

        [JsonProperty("gamma")]
        public decimal? Gamma { get; set; }

        [JsonProperty("rho")]
        public decimal? Rho { get; set; }

        [JsonProperty("theta")]
        public decimal? Theta { get; set; }

        [JsonProperty("vega")]
        public decimal? Vega { get; set; }
    }

    public class OptionTickerStats
    {
        [JsonProperty("high")]
        public decimal? High { get; set; }

        [JsonProperty("low")]
        public decimal? Low { get; set; }

        [JsonProperty("price_change")]
        public decimal? PriceChange { get; set; }

        [JsonProperty("volume")]
        public decimal? Volume { get; set; }

        [JsonProperty("volume_usd")]
        public decimal? VolumeUsd { get; set; }
    }
}
