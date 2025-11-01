using BaseStockConnector.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using BaseStockConnectorInterface.Models.Enums;

namespace BaseStockConnectorInterface.Models.Orders
{
    public class OrderCreateRequest
    {
        public string Instrument { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal Volume { get; set; }
        public Direction OrderDirection { get; set; }
        public OrderType Type { get; set; }
        public PositionOrderType PositionType { get; set; }
        public string ClientOrderId { get; set; } = string.Empty;
    }
}
