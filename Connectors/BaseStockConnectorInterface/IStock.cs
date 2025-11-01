using BaseStockConnector;
using BaseStockConnector.Models.Enums;

namespace BaseStockConnectorInterface
{
    public interface IStock<T>
    {
        string StockName { get; }

        IStockHttp HttpClient { get; set; }

        IStockSocket<T> SocketClient { get; set; }

    //TODO: Migrate to instance method
        static string GetInstrumentName(InstrumentType instrumentType, string baseAsset, string quoteAsset) => baseAsset + quoteAsset;
    }
}
