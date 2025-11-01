using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.RateLimiting;

public class RateLimitSnapshot
{
    public string ExchangeName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public List<RateLimitCurrentState> RateLimits { get; set; } = new();
}