using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnectorInterface.Models
{
    public class FundingRate
    {
        public string InstrumentName { get; set; } = string.Empty;
        public decimal FundingRateValue { get; set; }
        public decimal FundingRateInterval { get; set; }
        public decimal Premium { get; set; } = 0m;
        public DateTime NextFundingTime { get; set; }
    }
}
