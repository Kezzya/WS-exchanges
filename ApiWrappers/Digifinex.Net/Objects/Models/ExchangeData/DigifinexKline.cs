using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

[JsonConverter(typeof(ArrayConverter))]
public record DigifinexKline
{
    [ArrayProperty(0)]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime Timestamp { get; set; }

    [ArrayProperty(1)]
    public decimal Volume { get; set; }

    [ArrayProperty(2)]
    public decimal Close { get; set; }

    [ArrayProperty(3)]
    public decimal High { get; set; }

    [ArrayProperty(4)]
    public decimal Low { get; set; }

    [ArrayProperty(5)]
    public decimal Open { get; set; }
}