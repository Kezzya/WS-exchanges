using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Orders
{
    public class BaseOrderModel : BaseInstrumentModel
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public OrderErrorType ErrorType { get; set; } = OrderErrorType.Unknown;
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public OrderType Type { get; set; }
        public Direction Direction { get; set; }
        public DateTime TimeStamp { get; set; }
        public string StockOrderId { get; set; }
        public decimal FilledVolume { get; set; }
        public Guid SystemOrderId { get; set; }

        public enum OrderErrorType { None, Unknown, AlreadyCancelled, AlreadyFilled, ServiceUnavailable, BalanceNotEnough }
    }
}
