using CryptoExchange.Net.Objects;

namespace ChainApex.Net
{
    public class ChainApexEnvironment : TradeEnvironment
    {
        public string RestClientAddress { get; }
        public string PublicSocketClientAddress { get; }

        internal ChainApexEnvironment(
            string name,
            string restAddress,
            string publicSocketAddress) :
            base(name)
        {
            RestClientAddress = restAddress;
            PublicSocketClientAddress = publicSocketAddress;
        }

        public static ChainApexEnvironment Live { get; }
            = new ChainApexEnvironment(
                TradeEnvironmentNames.Live,
                "https://openapi.chainapex.pro",
                "wss://wspool.chainapex.pro/kline-api/ws");

        public static ChainApexEnvironment Test { get; }
            = new ChainApexEnvironment(
                "Test",
                "https://testnet-api.chainapex.pro",
                "wss://testnet-ws.chainapex.pro");

        public static ChainApexEnvironment CreateCustom(
            string name,
            string restAddress,
            string publicSocketAddress)
            => new ChainApexEnvironment(name, restAddress, publicSocketAddress);
    }
}
