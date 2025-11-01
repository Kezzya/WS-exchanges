using BaseStockConnector.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnectorInterface.Models
{
    public class Commission
    {
        public string InstrumentName { get; set; }
        public InstrumentType InstrumentType { get; set; }

        public decimal MakerFee { get; set; }
        public decimal TakerFee { get; set; }
        public decimal? BuyerFee { get; set; }
        public decimal? SellerFee { get; set; }
    }
}
