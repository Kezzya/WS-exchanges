using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.Account;

public class DigifinexOrder
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("created_date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("finished_date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? FinishedDate { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("cash_amount")]
    public decimal CashAmount { get; set; }

    [JsonProperty("executed_amount")]
    public decimal ExecutedAmount { get; set; }

    [JsonProperty("avg_price")]
    public decimal? AvgPrice { get; set; }

    [JsonProperty("status")]
    public DigifinexOrderStatus Status { get; set; }

    [JsonProperty("type")]
    public DigifinexOrderType Type { get; set; }

    [JsonProperty("kind")]
    public string Kind { get; set; } = string.Empty;
}