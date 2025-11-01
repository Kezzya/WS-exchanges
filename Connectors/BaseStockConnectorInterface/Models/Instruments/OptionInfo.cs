using BaseStockConnector.Models.Instruments;

using System;
using System.Collections.Generic;

namespace BaseStockConnectorInterface.Models.Instruments
{
    public class OptionInfo
    {
        public string InstrumentName { get; set; } = string.Empty;
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public OptionType OptionType { get; set; }
        public DateTime ExpirationTime { get; set; }
        public decimal Strike { get; set; }
        public List<OrderBookEntry> Asks { get; set; } = new List<OrderBookEntry>();
        public List<OrderBookEntry> Bids { get; set; } = new List<OrderBookEntry>();
        public OptionOrderBook OptionData{ get; set; }

        public override string ToString()
        {
            return InstrumentName;
        }
    }

    public enum OptionType
    {
        Call,
        Put
    }
}
