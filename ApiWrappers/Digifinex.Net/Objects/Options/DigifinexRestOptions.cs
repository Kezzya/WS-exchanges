// DigifinexRestOptions.cs
using CryptoExchange.Net.Objects.Options;

namespace Digifinex.Net.Objects.Options
{
    public class DigifinexRestOptions : RestExchangeOptions<DigifinexEnvironment>
    {
        public static DigifinexRestOptions Default { get; set; } = new DigifinexRestOptions()
        {
            Environment = DigifinexEnvironment.Live
        };

        public RestApiOptions SpotOptions { get; private set; } = new RestApiOptions();

        internal DigifinexRestOptions Copy()
        {
            var options = Copy<DigifinexRestOptions>();
            options.SpotOptions = SpotOptions.Copy<RestApiOptions>();
            return options;
        }
    }
}