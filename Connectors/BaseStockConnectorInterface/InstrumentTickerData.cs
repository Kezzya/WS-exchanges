using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockConnector
{
    class InstrumentTickerData
    {
        /// <summary>
        /// Full instrument name
        /// Example: BTC-3NOV21-65000-C
        /// </summary>
        public string? InstrumentName { get; set; }
        /// <summary>
        /// Volume 
        /// </summary>
        public decimal? Volume { get; set; }

        #region Greeks
        /// <summary>
        /// Delta
        /// </summary>
        public decimal? Delta { get; set; }
        /// <summary>
        /// Greeks Gamma
        /// </summary>
        public decimal? Gamma { get; set; }
        /// <summary>
        /// Greeks Theta
        /// </summary>
        public decimal? Theta { get; set; }
        /// <summary>
        /// Greeks Vega
        /// </summary>
        public decimal? Vega { get; set; }
        #endregion
        /// <summary>
        /// Implied Volatiility best ask
        /// </summary>
        public decimal? IVa { get; set; }
        /// <summary>
        /// Best ask price
        /// </summary>
        public decimal? Ask { get; set; }
        /// <summary>
        /// Implied Volatiility best bid
        /// </summary>
        public decimal? IVb { get; set; }
        /// <summary>
        /// Best bid price
        /// </summary>
        public decimal? Bid { get; set; }
        /// <summary>
        /// Last traded price
        /// </summary>
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// The linear relationship between option price and underlying asset price，leverage = Index Price *  Ratio / Option Mark Price * ABS (delta)
        /// </summary>
        public decimal? Leverage { get; set; }
        /// <summary>
        /// The number of contracts you have for the instrument
        /// </summary>
        public decimal? Position { get; set; }
        /// <summary>
        /// Change
        /// </summary>
        public decimal? Change { get; set; }
    }
}
