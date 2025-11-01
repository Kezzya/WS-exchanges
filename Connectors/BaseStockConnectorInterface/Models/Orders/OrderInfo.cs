using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models.Orders
{
    public class OrderInfo : BaseOrder
    {
        public DateTime TransactionDate { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public Direction Direction { get; set; }
        public string? TradeId { get; set; }
    }
}
