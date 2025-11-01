using System.Collections.Concurrent;

namespace Digifinex.Net;

public class DigifinexTradingRulesTracker
{
    private readonly ConcurrentDictionary<string, SymbolMetrics> _symbolMetrics = new();
    private readonly TimeSpan _calculationCycle = TimeSpan.FromMinutes(10);
    private readonly object _lockObject = new();

    public class SymbolMetrics
    {
        public List<OrderMetric> Orders { get; } = new();
    }

    public class OrderMetric
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime PlacedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal FilledAmount { get; set; }
        public bool IsFilled => FilledAmount > 0;
        public bool IsFullyCancelled => FilledAmount == 0 && CancelledAt.HasValue;
        public bool IsCancelledWithin5Seconds => CancelledAt.HasValue &&
                                                 (CancelledAt.Value - PlacedAt).TotalSeconds <= 5;
    }

    public void RecordOrderPlaced(string symbol, string orderId, decimal amount)
    {
        var metrics = _symbolMetrics.GetOrAdd(symbol, _ => new SymbolMetrics());

        lock (_lockObject)
        {
            CleanOldOrders(metrics);
            metrics.Orders.Add(new OrderMetric
            {
                OrderId = orderId,
                PlacedAt = DateTime.UtcNow,
                OrderAmount = amount
            });
        }
    }

    public void RecordOrderUpdate(string symbol, string orderId, decimal filledAmount)
    {
        if (!_symbolMetrics.TryGetValue(symbol, out var metrics))
            return;

        lock (_lockObject)
        {
            var order = metrics.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.FilledAmount = filledAmount;
            }
        }
    }

    public void RecordOrderCancelled(string symbol, string orderId)
    {
        if (!_symbolMetrics.TryGetValue(symbol, out var metrics))
            return;

        lock (_lockObject)
        {
            var order = metrics.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.CancelledAt = DateTime.UtcNow;
            }
        }
    }

    public (bool isAllowed, string? violationReason) CheckTradingRules(string symbol)
    {
        if (!_symbolMetrics.TryGetValue(symbol, out var metrics))
            return (true, null);

        lock (_lockObject)
        {
            CleanOldOrders(metrics);

            var orders = metrics.Orders;
            if (orders.Count == 0)
                return (true, null);

            // Calculate Filling Ratio (FR)
            if (orders.Count > 99)
            {
                var filledOrders = orders.Count(o => o.IsFilled);
                var fillingRatio = (decimal)filledOrders / orders.Count;

                if (fillingRatio < 0.01m)
                {
                    return (false, $"Filling Ratio violation: FR={fillingRatio:P2} < 1% with {orders.Count} orders");
                }
            }

            // Calculate Filling Weight (FW)
            if (orders.Count > 49)
            {
                var totalOrderAmount = orders.Sum(o => o.OrderAmount);
                var totalFilledAmount = orders.Sum(o => o.FilledAmount);

                if (totalOrderAmount > 0)
                {
                    var fillingWeight = totalFilledAmount / totalOrderAmount;
                    if (fillingWeight < 0.01m)
                    {
                        return (false, $"Filling Weight violation: FW={fillingWeight:P2} < 1% with {orders.Count} orders");
                    }
                }
            }

            // Calculate Cancellation Ratio (CR)
            if (orders.Count > 99)
            {
                var fullyCancelledWithin5Sec = orders.Count(o => o.IsFullyCancelled && o.IsCancelledWithin5Seconds);
                var cancellationRatio = (decimal)fullyCancelledWithin5Sec / orders.Count;

                if (cancellationRatio > 0.95m)
                {
                    return (false, $"Cancellation Ratio violation: CR={cancellationRatio:P2} > 95% with {orders.Count} orders");
                }
            }

            return (true, null);
        }
    }

    private void CleanOldOrders(SymbolMetrics metrics)
    {
        var cutoffTime = DateTime.UtcNow - _calculationCycle;
        metrics.Orders.RemoveAll(o => o.PlacedAt < cutoffTime);
    }

    public void Clear()
    {
        _symbolMetrics.Clear();
    }
}