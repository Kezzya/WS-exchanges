using BaseStockConnector.Models.Enums;

using System;

namespace BaseStockConnectorInterface.Models.Orders
{
    public class OrderTrade
    {
        public string OrderId { get; set; }
        public string InstrumentName { get; set; }
        public string TradeId { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public Direction Direction { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
        public decimal Fee { get; set; }
        public string FeeAsset { get; set; }
        public Role Role { get; set; }
    }
    public enum Role
    {
        Maker,
        Taker
    }
}
