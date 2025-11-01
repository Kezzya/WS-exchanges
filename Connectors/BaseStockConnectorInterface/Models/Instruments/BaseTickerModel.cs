using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Instruments
{
    public class BaseTickerModel
    {
        public string InstrumentName { get; set; }
        public string OriginalInstrumentName { get; set; }
        public string StockName { get; set; }
        public decimal? MarkPrice { get; set; }
        public string Error { get; set; }

    }
}
