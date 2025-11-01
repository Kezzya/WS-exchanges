using CryptoExchange.Net.Objects.Options;

namespace Digifinex.Net.Objects.Options
{
    public class DigifinexSocketOptions : SocketExchangeOptions<DigifinexEnvironment>
    {
        public static DigifinexSocketOptions Default { get; set; } = new DigifinexSocketOptions
        {
            Environment = DigifinexEnvironment.Live,
            SocketSubscriptionsCombineTarget = 10
        };

        public SocketApiOptions SpotOptions { get; private set; } = new SocketApiOptions();

        internal DigifinexSocketOptions Copy()
        {
            var options = Copy<DigifinexSocketOptions>();
            options.SpotOptions = SpotOptions.Copy<SocketApiOptions>();
            return options;
        }
    }
}