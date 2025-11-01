using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FameEX.Net.Objects.Models.Account
{
    public class FameexUserTrade
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("qty")]
        public string Amount { get; set; }

        [JsonProperty("commission")]
        public string Fee { get; set; }

        [JsonProperty("commissionAsset")]
        public string FeeCurrency { get; set; }

        [JsonProperty("time")]
        public long Timestamp { get; set; }

        [JsonProperty("isBuyer")]
        public bool IsBuyer { get; set; }

        [JsonProperty("isMaker")]
        public bool IsMaker { get; set; }

        public string Side => IsBuyer ? "buy" : "sell";
    }
}
