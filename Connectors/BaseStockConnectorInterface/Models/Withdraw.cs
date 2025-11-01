using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models
{
    public class Withdraw
    {
        public bool Success { get; set; } = false;
        public string ServerId { get; set; } = " ";
        public decimal Fee { get; set; }
        public string FeeAsset { get; set; } = String.Empty;
    }
}
