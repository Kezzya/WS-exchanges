using System;

namespace BaseStockConnectorInterface.Models.Kline
{
    public class KlineItem
    {
        public KlineItem()
        {

        }
        /// <summary>
        /// The time this candlestick opened
        /// </summary>
        public DateTime OpenTime { get; set; }

        /// <summary>
        /// The price at which this candlestick opened
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// The highest price in this candlestick
        /// </summary>
        public decimal HighPrice { get; set; }

        /// <summary>
        /// The lowest price in this candlestick
        /// </summary>
        public decimal LowPrice { get; set; }

        /// <summary>
        /// The price at which this candlestick closed
        /// </summary>
        public decimal ClosePrice { get; set; }

        /// <summary>
        /// The volume traded during this candlestick
        /// </summary>
        public decimal Volume { get; set; }
    }
}
