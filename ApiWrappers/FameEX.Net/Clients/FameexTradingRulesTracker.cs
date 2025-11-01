using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FameEX.Net.Clients.SpotApi
{
    /// <summary>
    /// Tracks trading rules and order states to prevent violations of exchange limits
    /// </summary>
    public class FameexTradingRulesTracker
    {
        // Tracking data per symbol
        private readonly ConcurrentDictionary<string, SymbolTrackingData> _symbolTracking = new();

        // Default trading rules (can be configured based on exchange requirements)
        private readonly TimeSpan _orderPlacementWindow = TimeSpan.FromSeconds(1);
        private readonly int _maxOrdersPerSecond = 10;
        private readonly int _maxOpenOrdersPerSymbol = 200;
        private readonly TimeSpan _minTimeBetweenOrders = TimeSpan.FromMilliseconds(100);

        private class SymbolTrackingData
        {
            public ConcurrentDictionary<string, OrderData> ActiveOrders { get; } = new();
            public ConcurrentQueue<DateTime> RecentOrderTimestamps { get; } = new();
            public DateTime? LastOrderTime { get; set; }
        }

        private class OrderData
        {
            public string OrderId { get; set; }
            public decimal Amount { get; set; }
            public decimal FilledAmount { get; set; }
            public DateTime PlacedTime { get; set; }
        }

        /// <summary>
        /// Check if placing an order would violate trading rules
        /// </summary>
        public (bool IsAllowed, string ViolationReason) CheckTradingRules(string symbol)
        {
            var tracking = _symbolTracking.GetOrAdd(symbol, _ => new SymbolTrackingData());

            // Check 1: Too many open orders
            if (tracking.ActiveOrders.Count >= _maxOpenOrdersPerSymbol)
            {
                return (false, $"Maximum open orders limit reached ({_maxOpenOrdersPerSymbol})");
            }

            // Check 2: Order rate limiting (orders per second)
            CleanupOldTimestamps(tracking.RecentOrderTimestamps, _orderPlacementWindow);

            if (tracking.RecentOrderTimestamps.Count >= _maxOrdersPerSecond)
            {
                return (false, $"Order rate limit exceeded ({_maxOrdersPerSecond} per second)");
            }

            // Check 3: Minimum time between orders
            if (tracking.LastOrderTime.HasValue)
            {
                var timeSinceLastOrder = DateTime.UtcNow - tracking.LastOrderTime.Value;
                if (timeSinceLastOrder < _minTimeBetweenOrders)
                {
                    var waitTime = (_minTimeBetweenOrders - timeSinceLastOrder).TotalMilliseconds;
                    return (false, $"Too fast. Wait {waitTime:F0}ms before next order");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Record that an order has been placed
        /// </summary>
        public void RecordOrderPlaced(string symbol, string orderId, decimal amount)
        {
            var tracking = _symbolTracking.GetOrAdd(symbol, _ => new SymbolTrackingData());

            var orderData = new OrderData
            {
                OrderId = orderId,
                Amount = amount,
                FilledAmount = 0,
                PlacedTime = DateTime.UtcNow
            };

            tracking.ActiveOrders.TryAdd(orderId, orderData);
            tracking.RecentOrderTimestamps.Enqueue(DateTime.UtcNow);
            tracking.LastOrderTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Record an order update (partial or full fill)
        /// </summary>
        public void RecordOrderUpdate(string symbol, string orderId, decimal filledAmount)
        {
            if (_symbolTracking.TryGetValue(symbol, out var tracking))
            {
                if (tracking.ActiveOrders.TryGetValue(orderId, out var orderData))
                {
                    orderData.FilledAmount = filledAmount;

                    // If fully filled, remove from active orders
                    if (filledAmount >= orderData.Amount)
                    {
                        tracking.ActiveOrders.TryRemove(orderId, out _);
                    }
                }
            }
        }

        /// <summary>
        /// Record that an order has been cancelled
        /// </summary>
        public void RecordOrderCancelled(string symbol, string orderId)
        {
            if (_symbolTracking.TryGetValue(symbol, out var tracking))
            {
                tracking.ActiveOrders.TryRemove(orderId, out _);
            }
        }

        /// <summary>
        /// Get current number of active orders for a symbol
        /// </summary>
        public int GetActiveOrderCount(string symbol)
        {
            if (_symbolTracking.TryGetValue(symbol, out var tracking))
            {
                return tracking.ActiveOrders.Count;
            }
            return 0;
        }

        /// <summary>
        /// Get current order rate (orders per second)
        /// </summary>
        public int GetCurrentOrderRate(string symbol)
        {
            if (_symbolTracking.TryGetValue(symbol, out var tracking))
            {
                CleanupOldTimestamps(tracking.RecentOrderTimestamps, _orderPlacementWindow);
                return tracking.RecentOrderTimestamps.Count;
            }
            return 0;
        }

        /// <summary>
        /// Clear all tracking data for a symbol
        /// </summary>
        public void ClearSymbol(string symbol)
        {
            _symbolTracking.TryRemove(symbol, out _);
        }

        /// <summary>
        /// Clear all tracking data
        /// </summary>
        public void ClearAll()
        {
            _symbolTracking.Clear();
        }

        /// <summary>
        /// Get statistics for a symbol
        /// </summary>
        public TradingStatistics GetStatistics(string symbol)
        {
            if (_symbolTracking.TryGetValue(symbol, out var tracking))
            {
                CleanupOldTimestamps(tracking.RecentOrderTimestamps, _orderPlacementWindow);

                return new TradingStatistics
                {
                    Symbol = symbol,
                    ActiveOrderCount = tracking.ActiveOrders.Count,
                    RecentOrderRate = tracking.RecentOrderTimestamps.Count,
                    LastOrderTime = tracking.LastOrderTime,
                    MaxOpenOrders = _maxOpenOrdersPerSymbol,
                    MaxOrdersPerSecond = _maxOrdersPerSecond
                };
            }

            return new TradingStatistics
            {
                Symbol = symbol,
                ActiveOrderCount = 0,
                RecentOrderRate = 0,
                LastOrderTime = null,
                MaxOpenOrders = _maxOpenOrdersPerSymbol,
                MaxOrdersPerSecond = _maxOrdersPerSecond
            };
        }

        /// <summary>
        /// Remove timestamps older than the specified window
        /// </summary>
        private void CleanupOldTimestamps(ConcurrentQueue<DateTime> queue, TimeSpan window)
        {
            var cutoffTime = DateTime.UtcNow - window;

            while (queue.TryPeek(out var timestamp) && timestamp < cutoffTime)
            {
                queue.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Cleanup old orders that are likely closed but we missed the update
        /// </summary>
        public void CleanupStaleOrders(TimeSpan maxAge)
        {
            var cutoffTime = DateTime.UtcNow - maxAge;

            foreach (var symbolTracking in _symbolTracking.Values)
            {
                var staleOrders = symbolTracking.ActiveOrders
                    .Where(kvp => kvp.Value.PlacedTime < cutoffTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var orderId in staleOrders)
                {
                    symbolTracking.ActiveOrders.TryRemove(orderId, out _);
                }
            }
        }
    }

    /// <summary>
    /// Statistics about trading activity for a symbol
    /// </summary>
    public class TradingStatistics
    {
        public string Symbol { get; set; }
        public int ActiveOrderCount { get; set; }
        public int RecentOrderRate { get; set; }
        public DateTime? LastOrderTime { get; set; }
        public int MaxOpenOrders { get; set; }
        public int MaxOrdersPerSecond { get; set; }

        public bool IsNearLimit => ActiveOrderCount >= (MaxOpenOrders * 0.9);
        public bool IsRateLimited => RecentOrderRate >= MaxOrdersPerSecond;
    }
}