using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models
{
    public class WithdrawHistory
    {
        public string Ticker { get; set; }
        public decimal Amount { get; set; }
        public string Address { get; set; } 
        public string? UniqueId { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Fee { get; set; } = 0;
        public string FeeAsset { get; set; } = string.Empty;

    }
}
