using BaseStockConnector.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models.Orders
{
    public class OrderSyncInfo
    {
        public OrderSyncInfo()
        {

        }

        public string? StockOrderId { get; set; }
        public OrderHistoryItemStatus Status { get; set; }

        public decimal Price { get; set; }
        public decimal? AvgPrice { get; set; }
        public decimal Volume { get; set; }
        public decimal FilledVolume { get; set; }
        public Direction? Direction { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? InstrumentName { get; set; }
        public decimal? Fee { get; set; }
        public string FeeAsset { get; set; }
        public OrderType? OrderType { get; set; }

    }

    public enum OrderSyncInfoStatus
    {
        None,
        /// <summary>
        /// Order is new
        /// </summary>
        Open,
        /// <summary>
        /// Order is partly filled, still has quantity left to fill
        /// </summary>
        PartiallyFilled,
        /// <summary>
        /// The order has been filled and completed
        /// </summary>
        Filled,
        /// <summary>
        /// The order has been canceled
        /// </summary>
        Canceled,
    }
}
