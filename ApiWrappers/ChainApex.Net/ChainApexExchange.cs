using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Objects;

namespace ChainApex.Net
{
    public static class ChainApexExchange
    {
        public static string ExchangeName => "ChainApex";
        public static string Url { get; } = "https://openapi.chainapex.pro/";
        public static string[] ApiDocsUrl { get; } = new[] { "https://exchangeopenapi.gitbook.io/pri-openapi/" };
        
        public static ChainApexRateLimiters RateLimiter { get; } = new ChainApexRateLimiters();
    }

    public class ChainApexRateLimiters
    {
        public event Action<RateLimitEvent>? RateLimitTriggered;

        internal IRateLimitGate WeightLimit { get; private set; } = null!;

        internal ChainApexRateLimiters()
        {
            Initialize();
        }

        private void Initialize()
        {
            // ChainApex rate limiting - based on documentation
            // Typically 1200 requests per minute for authenticated endpoints
            WeightLimit = new RateLimitGate("Weight")
                .AddGuard(new RateLimitGuard(RateLimitGuard.PerHostPerProxy, new List<IGuardFilter>(), 1200, TimeSpan.FromMinutes(1), RateLimitWindowType.Sliding));
            
            WeightLimit.RateLimitTriggered += (x) => RateLimitTriggered?.Invoke(x);
        }

        public Dictionary<string, IRateLimitGate> GetGatesDictionary()
        {
            return new Dictionary<string, IRateLimitGate>
            {
                { "Weight", WeightLimit }
            };
        }
    }
}
