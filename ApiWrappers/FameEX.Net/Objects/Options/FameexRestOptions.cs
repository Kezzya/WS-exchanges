using CryptoExchange.Net.Objects.Options;

namespace FameEX.Net.Objects.Options
{
    public class FameexRestOptions : RestExchangeOptions<FameexEnvironment>
    {
        public static FameexRestOptions Default { get; set; } = new FameexRestOptions
        {
            Environment = FameexEnvironment.Live,
            AutoTimestamp = true
        };

        public RestApiOptions SpotOptions { get; private set; } = new RestApiOptions();

        internal FameexRestOptions Copy()
        {
            var options = Copy<FameexRestOptions>();
            options.SpotOptions = SpotOptions.Copy<RestApiOptions>();
            return options;
        }
    }
}