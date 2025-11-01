using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

[JsonConverter(typeof(ArrayConverter))]
public record DigifinexOrderBookEntry : ISymbolOrderBookEntry
{
    [ArrayProperty(0)]
    public decimal Price { get; set; }

    [ArrayProperty(1)]
    public decimal Quantity { get; set; }
}