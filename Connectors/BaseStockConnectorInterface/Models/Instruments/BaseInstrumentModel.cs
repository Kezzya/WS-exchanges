using BaseStockConnector.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Instruments
{
    public class BaseInstrumentModel : IBaseInstrumentModel
    {
        /// <summary>
        /// Full instrument name
        /// Example: BTC-3NOV21-65000-C
        /// </summary>
        public string InstrumentName { get; set; }
        public string OriginalInstrumentName { get; set; }
        public InstrumentType InstrumentType { get; set; }
        public DateTime? ExpirationTimeUTC { get; set; }
        public string StockName { get; set; }
    }
}
