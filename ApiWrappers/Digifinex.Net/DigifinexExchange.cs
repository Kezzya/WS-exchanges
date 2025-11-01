using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Objects;

namespace Digifinex.Net
{
    public static class DigifinexExchange
    {
        public static string ExchangeName => "Digifinex";
        public static string Url { get; } = "https://www.digifinex.com/";
        public static string[] ApiDocsUrl { get; } = new[] { "https://www.digifinex.com/api_docs" };
        
        public static DigifinexRateLimiters RateLimiter { get; } = new DigifinexRateLimiters();
    }

    public class DigifinexRateLimiters
    {
        public event Action<RateLimitEvent>? RateLimitTriggered;

        internal DigifinexRateLimiters()
        {
            Initialize();
        }

        private void Initialize()
        {
            WeightLimit = new RateLimitGate("Weight")
                .AddGuard(new RateLimitGuard(RateLimitGuard.PerHostPerProxy, new List<IGuardFilter>(), 1200, TimeSpan.FromMinutes(1), RateLimitWindowType.Sliding));
            
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