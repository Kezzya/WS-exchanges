using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.RateLimiting.Interfaces;
using Microsoft.Extensions.Logging;
using Digifinex.Net.Clients.SpotApi;
using Digifinex.Net.Objects.Options;

namespace Digifinex.Net.Clients
{
    public class DigifinexRestClient : BaseRestClient
    {
        public DigifinexRestClientSpotApi SpotApi { get; }

        internal DigifinexRestClient(Action<DigifinexRestOptions>? optionsDelegate = null) 
            : this(null, null, optionsDelegate)
        {
        }

        public DigifinexRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, Action<DigifinexRestOptions>? optionsDelegate = null) 
            : base(loggerFactory, "Digifinex")
        {
            var options = DigifinexRestOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new DigifinexRestClientSpotApi(_logger, httpClient, options));
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
        }

        public static void SetDefaultOptions(Action<DigifinexRestOptions> optionsFunc)
        {
            var options = DigifinexRestOptions.Default.Copy();
            optionsFunc(options);
            DigifinexRestOptions.Default = options;
        }

        protected override Dictionary<string, IRateLimitGate> GetRateLimitGates()
        {
            return DigifinexExchange.RateLimiter.GetGatesDictionary();
        }
    }
}