using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Orders
{
    public class OrderUpdate : BaseOrder
    {
        public string StockOrderId { get; set; }
        public string SystemOrderId { get; set; }
        public decimal Price { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Direction OrderDirection { get; set; }
        public OrderType OrderType { get; set; }
        public decimal FilledVolume { get; set; }
        public long Timestamp { get; set; }
        public DateTime EventTimeStamp { get; set; }
        public decimal MarkPrice { get; set; }
        public decimal IndexPrice { get; set; }
        public decimal Fee { get; set; }
        public string FeeAsset { get; set; }
    }
}
