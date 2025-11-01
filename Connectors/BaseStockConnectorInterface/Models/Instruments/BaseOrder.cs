using BaseStockConnector.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Instruments
{
    public class BaseOrder
    {
        public string InstrumentName { get; set; }
        public string OriginalInstrumentName { get; set; }
        public string StockName { get; set; }
        public InstrumentType InstrumentType { get; set; }
    }
}
