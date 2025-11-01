using CryptoExchange.Net.Objects.Options;

namespace FameEX.Net.Objects.Options
{
    public class FameexSocketOptions : SocketExchangeOptions<FameexEnvironment>
    {
        public static FameexSocketOptions Default { get; set; } = new FameexSocketOptions
        {
            Environment = FameexEnvironment.Live,
            SocketSubscriptionsCombineTarget = 10,
            OutputOriginalData = false
        };

        public SocketApiOptions SpotOptions { get; private set; } = new SocketApiOptions();

        internal FameexSocketOptions Copy()
        {
            var options = Copy<FameexSocketOptions>();
            options.SpotOptions = SpotOptions.Copy<SocketApiOptions>();
            return options;
        }
    }
}