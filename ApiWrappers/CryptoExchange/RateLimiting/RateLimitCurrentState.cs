using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Current rate limit state
    /// </summary>
    public class RateLimitCurrentState
    {
        public string TrackerKey { get; set; }

        public RateLimitTrackerMetadata? TrackerMetadata { get; set; }

        public int Limit { get; set; }

        public TimeSpan Period { get; set; }

        public int Current { get; set; }
    }
}