using BaseStockConnector.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Instruments
{
    public class OrderBook : BaseOrder
    {
        public List<OrderBookEntry> Asks { get; set; } = new List<OrderBookEntry>();
        public List<OrderBookEntry> Bids { get; set; } = new List<OrderBookEntry>();
        /// <summary>
        /// Data available only for Option
        /// </summary>
        public OptionOrderBook? OptionData { get; set; }
        public DateTime StockEventTime { get; set; }
        public DateTime SystemTime { get; set; }
    }
    public class OrderBookEntry
    {
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }

    public class OptionOrderBook
    {
        public OptionGreeks Greeks { get; set; }

        public decimal MarkIv { get; set; }
        public decimal AskIv { get; set; }
        public decimal BidIv { get; set; }
    }

    public class OptionGreeks
    {
        public decimal Delta { get; set; }
        public decimal Gamma { get; set; }
        public decimal Vega { get; set; }
        public decimal Theta { get; set; }
        public decimal Rho { get; set; }
    }
}
