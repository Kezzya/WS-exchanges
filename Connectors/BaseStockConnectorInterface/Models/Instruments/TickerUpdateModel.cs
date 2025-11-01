using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models.Instruments
{
    public class BaseTickerUpdateModel
    {
        public string StockName { get; set; }
        public string InstrumentName { get; set; }
    }
    public class TickerUpdateModel : BaseTickerUpdateModel
    {
        public decimal LastPrice { get; set; }
        public decimal TotalVolume { get; set; }
    }
    public class PriceUpdateModel: BaseTickerUpdateModel
    {
        public decimal LastPrice { get; set; }
    }
    public class TotalVolumeUpdateVolume : BaseTickerUpdateModel
    {
        public decimal TotalVolume { get; set; }
    }
}
