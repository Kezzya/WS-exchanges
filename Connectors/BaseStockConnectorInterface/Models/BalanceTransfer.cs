using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models
{
    public class BalanceTransfer
    {
        public bool Confirmed { get; set; }
        public string Error { get; set; }
        public string ServerId { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public string FeeAsset { get; set; } = String.Empty;

    }
}
