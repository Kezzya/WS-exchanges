using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Models
{
    public class BalancesModel
    {
        public string StockName { get; set; }
        public List<Balance> Balances { get; set; }

    }
    public class Balance
    {
        public string Currency { get; set; }
        public decimal AvalibleBalance { get; set; }
        public decimal FreezedBalance { get; set; }
    }
}
