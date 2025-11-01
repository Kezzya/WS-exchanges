using CryptoExchange.Net.Objects.Options;

namespace ChainApex.Net.Objects.Options
{
    public class ChainApexSocketOptions : SocketExchangeOptions<ChainApexEnvironment>
    {
        public static ChainApexSocketOptions Default { get; set; } = new ChainApexSocketOptions
        {
            Environment = ChainApexEnvironment.Live,
            SocketSubscriptionsCombineTarget = 10
        };

        public SocketApiOptions SpotOptions { get; private set; } = new SocketApiOptions();

        internal ChainApexSocketOptions Copy()
        {
            var options = Copy<ChainApexSocketOptions>();
            options.SpotOptions = SpotOptions.Copy<SocketApiOptions>();
            return options;
        }
    }
}
