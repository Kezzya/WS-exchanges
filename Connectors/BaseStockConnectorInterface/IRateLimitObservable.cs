using System.Threading.Tasks;
using CryptoExchange.Net.RateLimiting;

namespace BaseStockConnectorInterface
{
    public interface IRateLimitObservable
    {
        public Task<RateLimitSnapshot> GetRateLimitSnapshot();
    }
}
