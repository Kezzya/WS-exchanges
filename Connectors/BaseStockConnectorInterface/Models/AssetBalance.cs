namespace BaseStockConnectorInterface.Models
{
    public class AssetBalance
    {
        public string Currency { get; set; } = string.Empty;
        public decimal AvailableBalance { get; set; }
        public decimal FrozenBalance { get; set; }
    }
}
