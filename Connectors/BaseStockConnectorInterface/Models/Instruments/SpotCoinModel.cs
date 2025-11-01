using BaseStockConnector.Models.Enums;

namespace BaseStockConnector.Models.Instruments
{
    public class SpotCoinModel : IBaseInstrumentModel
    {
        public string InstrumentName { get; set; }
        
        public string OriginalInstrumentName { get; set; }
        
        public InstrumentType InstrumentType { get; set; }
        
        public string StockName { get; set; }
        
        /// <summary>
        /// Шаг цены
        /// </summary>
        public decimal MinMovement { get; set; }
        
        /// <summary>
        /// Шаг объемов
        /// </summary>
        public decimal MinMovementVolume { get; set; }
        
        public decimal? MinVolumeInBaseCurrency { get; set; }

        public decimal MinVolumeInQuoteCurrency { get; set; } = -1m;
        
        public string BaseCurrency { get; set; }
        
        public string QuoteCurrency { get; set; }
    }
}
