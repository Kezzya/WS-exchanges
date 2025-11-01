using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.RateLimiting.Interfaces;
using Microsoft.Extensions.Logging;
using FameEX.Net.Clients.SpotApi;
using FameEX.Net.Objects.Options;

namespace FameEX.Net.Clients
{
    public class FameexRestClient : BaseRestClient
    {
        public FameexRestClientSpotApi SpotApi { get; }

        internal FameexRestClient(Action<FameexRestOptions>? optionsDelegate = null)
            : this(null, null, optionsDelegate)
        {
        }

        public FameexRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, Action<FameexRestOptions>? optionsDelegate = null)
            : base(loggerFactory, "FameEX")
        {
            var options = FameexRestOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new FameexRestClientSpotApi(_logger, httpClient, options));
        }

        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
        }

        public static void SetDefaultOptions(Action<FameexRestOptions> optionsFunc)
        {
            var options = FameexRestOptions.Default.Copy();
            optionsFunc(options);
            FameexRestOptions.Default = options;
        }

        protected override Dictionary<string, IRateLimitGate> GetRateLimitGates()
        {
            return FameexExchange.RateLimiter.GetGatesDictionary();
        }

        public async Task GetOrderAsync(string orderId, string v)
        {
            throw new NotImplementedException();
        }
    }
}