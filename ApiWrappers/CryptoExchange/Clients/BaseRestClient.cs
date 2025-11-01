using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// Base rest client
    /// </summary>
    public abstract class BaseRestClient : BaseClient, IRestClient
    {
        /// <inheritdoc />
        public int TotalRequestsMade => ApiClients.OfType<RestApiClient>().Sum(s => s.TotalRequestsMade);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="name">The name of the API this client is for</param>
        protected BaseRestClient(ILoggerFactory? loggerFactory, string name) : base(loggerFactory, name)
        {
        }

        /// <summary>
        /// Get rate limit gates used by this client. Must be implemented by derived classes.
        /// </summary>
        /// <returns>Dictionary of gate name to gate instance</returns>
        protected abstract Dictionary<string, IRateLimitGate> GetRateLimitGates();

        /// <summary>
        /// Get current rate limit information for all gates
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Current rate limit snapshot</returns>
        public async Task<RateLimitSnapshot> GetCurrentRateLimitsAsync(CancellationToken ct = default)
        {
            var rateLimits = new List<RateLimitCurrentState>();
            var rlGates = GetRateLimitGates();
            
            //Gates
            foreach (var rlGate in rlGates)
            {
                var guardLimits = await rlGate.Value.GetCurrentRateLimitStatesAsync();

                //Guards
                foreach (var guard in guardLimits)
                {
                    //States
                    rateLimits.AddRange(guard.Value.Select(gs => gs.Value));
                }
            }

            var rateLimitsSnapshot = new RateLimitSnapshot()
            {
                Timestamp = DateTime.UtcNow,
                RateLimits = rateLimits
            };

            return rateLimitsSnapshot;
        }
    }
}
