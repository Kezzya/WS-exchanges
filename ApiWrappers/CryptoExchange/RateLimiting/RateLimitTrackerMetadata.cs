namespace CryptoExchange.Net.RateLimiting;

/// <summary>
/// Metadata stored for each tracker
/// </summary>
public class RateLimitTrackerMetadata
{
    public string? Proxy { get; set; }

    public string? Host { get; set; }

    public string? Path { get; set; }

    public string? Method { get; set; }
}