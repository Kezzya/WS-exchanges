using BaseStockConnector.Models.Enums;

using BaseStockConnectorInterface.Models.Instruments;

namespace BaseStockConnectorInterface.Models.Orders
{
    public class Position
    {
        public InstrumentType InstrumentType { get; set; }
        public string InstrumentName { get; set; }
        public OptionType OptionType { get; set; }
        public Direction? Direction { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MarkPrice { get; set; }
        public decimal Size { get; set; }
        public decimal SizeCurrency { get; set; }
        public decimal Delta { get; set; }
        public decimal Gamma { get; set; }
        public decimal Theta { get; set; }
        public decimal Vega { get; set; }
        public decimal Rho { get; set; }
    }
}
