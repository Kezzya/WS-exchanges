using CryptoExchange.Net.Attributes;

namespace Digifinex.Net.Objects.Models.Account;

public enum DigifinexOrderType
{
    [Map("buy")]
    Buy,
    [Map("sell")]
    Sell,
    [Map("buy_market")]
    BuyMarket,
    [Map("sell_market")]
    SellMarket
}