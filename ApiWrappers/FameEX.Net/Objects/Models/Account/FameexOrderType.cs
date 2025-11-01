using CryptoExchange.Net.Attributes;

namespace FameEX.Net.Objects.Models.Account
{
    public enum FameexOrderType
    {
        [Map("LIMIT")]
        Limit,
        [Map("MARKET")]
        Market
    }
}