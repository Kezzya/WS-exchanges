using CryptoExchange.Net.Objects.Options;

namespace ChainApex.Net.Objects.Options
{
    public class ChainApexRestOptions : RestExchangeOptions<ChainApexEnvironment>
    {
        public static ChainApexRestOptions Default { get; set; } = new ChainApexRestOptions()
        {
            Environment = ChainApexEnvironment.Live
        };

        public RestApiOptions SpotOptions { get; private set; } = new RestApiOptions();

        internal ChainApexRestOptions Copy()
        {
            var options = Copy<ChainApexRestOptions>();
            options.SpotOptions = SpotOptions.Copy<RestApiOptions>();
            return options;
        }
    }
}
