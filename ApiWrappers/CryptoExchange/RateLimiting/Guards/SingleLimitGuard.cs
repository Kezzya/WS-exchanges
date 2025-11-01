using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Rate limit guard for a per endpoint limit
    /// </summary>
    public class SingleLimitGuard : IRateLimitGuard
    {
        /// <summary>
        /// Default endpoint limit
        /// </summary>
        public static Func<RequestDefinition, string, SecureString?, string> Default { get; } = new Func<RequestDefinition, string, SecureString?, string>((def, host, key) => host + def.Path + def.Method);

        /// <summary>
        /// Endpoint limit per API key
        /// </summary>
        public static Func<RequestDefinition, string, SecureString?, string> PerApiKey { get; } = new Func<RequestDefinition, string, SecureString?, string>((def, host, key) => host + def.Path + def.Method + key!.GetString());
        private static readonly ConcurrentDictionary<string, IWindowTracker> _trackers = new ConcurrentDictionary<string, IWindowTracker>();
        private static readonly ConcurrentDictionary<string, RateLimitTrackerMetadata> _trackerMetadata = new ConcurrentDictionary<string, RateLimitTrackerMetadata>();
        private readonly RateLimitWindowType _windowType;
        private readonly double? _decayRate;
        private readonly int _limit;
        private readonly TimeSpan _period;
        private readonly Func<RequestDefinition, string, SecureString?, string> _keySelector;

        /// <inheritdoc />
        public string Name { get; set; } = "EndpointLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit requests to endpoint {_limit} per {_period}";

        /// <summary>
        /// ctor
        /// </summary>
        public SingleLimitGuard(
            int limit,
            TimeSpan period,
            RateLimitWindowType windowType,
            double? decayRate = null,
            Func<RequestDefinition, string, SecureString?, string>? keySelector = null, string? name = null)
        {
            _limit = limit;
            _period = period;
            _windowType = windowType;
            _decayRate = decayRate;
            _keySelector = keySelector ?? Default;
            Name = name ?? Name;
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var key = _keySelector(definition, host, apiKey);
            if (!_trackers.TryGetValue(key, out var tracker))
            {
                tracker = CreateTracker();
                _trackers.TryAdd(key, tracker);
                _trackerMetadata.TryAdd(key, new RateLimitTrackerMetadata
                {
                    Proxy = definition.Proxy?.ToString(),
                    Host = host,
                    Path = definition.Path,
                    Method = definition.Method.ToString()
                });
            }

            var delay = tracker.GetWaitTime(requestWeight);
            if (delay == default)
                return LimitCheck.NotNeeded;

            return LimitCheck.Needed(delay, _limit, _period, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var key = _keySelector(definition, host, apiKey);
            var tracker = _trackers[key];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_limit, _period, tracker.Current);
        }

        /// <summary>
        /// Create a new WindowTracker
        /// </summary>
        /// <returns></returns>
        protected IWindowTracker CreateTracker()
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(_limit, _period)
                : _windowType == RateLimitWindowType.Fixed ? new FixedWindowTracker(_limit, _period) :
                new DecayWindowTracker(_limit, _period, _decayRate ?? throw new InvalidOperationException("Decay rate not provided"));
        }

        /// <inheritdoc />
        public Dictionary<string, RateLimitCurrentState> GetAllCurrentStates()
        {
            var states = new Dictionary<string, RateLimitCurrentState>();
            foreach (var t in _trackers)
            {
                _ = _trackerMetadata.TryGetValue(t.Key, out var tm);

                //TrackerKey, State
                states[t.Key] = new RateLimitCurrentState()
                {
                    TrackerKey = t.Key,
                    TrackerMetadata = tm,
                    Current = t.Value.Current,
                    Limit = t.Value.Limit,
                    Period = t.Value.TimePeriod
                };
            }

            return states;
        }
    }
}
