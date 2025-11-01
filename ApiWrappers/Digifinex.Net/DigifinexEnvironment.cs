using CryptoExchange.Net.Objects;

namespace Digifinex.Net
{
    public class DigifinexEnvironment : TradeEnvironment
    {
        public string RestClientAddress { get; }
        public string PublicSocketClientAddress { get; }

        internal DigifinexEnvironment(
            string name,
            string restAddress,
            string publicSocketAddress) :
            base(name)
        {
            RestClientAddress = restAddress;
            PublicSocketClientAddress = publicSocketAddress;
        }

        public static DigifinexEnvironment Live { get; }
            = new DigifinexEnvironment(
                TradeEnvironmentNames.Live,
                "https://openapi.digifinex.com",
                "wss://openapi.digifinex.com/ws/v1/");

        public static DigifinexEnvironment CreateCustom(
            string name,
            string restAddress,
            string publicSocketAddress)
            => new DigifinexEnvironment(name, restAddress, publicSocketAddress);
    }
}