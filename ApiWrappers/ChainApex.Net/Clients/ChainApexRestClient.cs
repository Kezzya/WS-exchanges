using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.RateLimiting.Interfaces;
using Microsoft.Extensions.Logging;
using ChainApex.Net.Clients.SpotApi;
using ChainApex.Net.Objects.Options;

namespace ChainApex.Net.Clients
{
    public class ChainApexRestClient : BaseRestClient
    {
        public ChainApexRestClientSpotApi SpotApi { get; }

        internal ChainApexRestClient(Action<ChainApexRestOptions>? optionsDelegate = null) 
            : this(null, null, optionsDelegate)
        {
        }

        public ChainApexRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, Action<ChainApexRestOptions>? optionsDelegate = null) 
            : base(loggerFactory, "ChainApex")
        {
            var options = ChainApexRestOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new ChainApexRestClientSpotApi(_logger, httpClient, options));
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
        }

        public static void SetDefaultOptions(Action<ChainApexRestOptions> optionsFunc)
        {
            var options = ChainApexRestOptions.Default.Copy();
            optionsFunc(options);
            ChainApexRestOptions.Default = options;
        }

        protected override Dictionary<string, IRateLimitGate> GetRateLimitGates()
        {
            return ChainApexExchange.RateLimiter.GetGatesDictionary();
        }
    }
}
