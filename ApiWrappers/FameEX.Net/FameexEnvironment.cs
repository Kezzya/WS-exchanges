using CryptoExchange.Net.Objects;

namespace FameEX.Net
{
    public class FameexEnvironment : TradeEnvironment
    {
        public string RestClientAddress { get; }
        public string PublicSocketClientAddress { get; }

        internal FameexEnvironment(
            string name,
            string restAddress,
            string publicSocketAddress) :
            base(name)
        {
            RestClientAddress = restAddress;
            PublicSocketClientAddress = publicSocketAddress;
        }

        public static FameexEnvironment Live { get; }
            = new FameexEnvironment(
                TradeEnvironmentNames.Live,
                "https://openapi.fameex.com",
                "wss://ws.fameex.com/kline-api/ws");

        public static FameexEnvironment CreateCustom(
            string name,
            string restAddress,
            string publicSocketAddress)
            => new FameexEnvironment(name, restAddress, publicSocketAddress);
    }
}