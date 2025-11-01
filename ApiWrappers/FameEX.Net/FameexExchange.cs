using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Objects;

namespace FameEX.Net
{
    public static class FameexExchange
    {
        public static string ExchangeName => "FameEX";
        public static string Url { get; } = "https://www.fameex.com/";
        public static string[] ApiDocsUrl { get; } = new[] { "https://fameexdocs.github.io/docs-v1/en/index.html" };

        public static FameexRateLimiters RateLimiter { get; } = new FameexRateLimiters();
    }

    public class FameexRateLimiters
    {
        public event Action<RateLimitEvent>? RateLimitTriggered;

        internal FameexRateLimiters()
        {
            Initialize();
        }

        private void Initialize()
        {
            WeightLimit = new RateLimitGate("Weight")
                .AddGuard(new RateLimitGuard(RateLimitGuard.PerHostPerProxy,
                    new List<IGuardFilter>(),
                    1200,
                    TimeSpan.FromMinutes(1),
                    RateLimitWindowType.Sliding));

            WeightLimit.RateLimitTriggered += (x) => RateLimitTriggered?.Invoke(x);
        }

        internal IRateLimitGate WeightLimit { get; private set; }

        public Dictionary<string, IRateLimitGate> GetGatesDictionary()
        {
            return new Dictionary<string, IRateLimitGate>
            {
                { "Weight", WeightLimit }
            };
        }
    }
}