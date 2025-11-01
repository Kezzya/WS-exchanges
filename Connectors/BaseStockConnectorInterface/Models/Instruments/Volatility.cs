using System;

namespace BaseStockConnectorInterface.Models.Instruments
{
    public class Volatility
    {
        public string BaseCurrency { get; set; } = null!;
        public string? QuoteCurrency { get; set; }
        public DateTime Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}
