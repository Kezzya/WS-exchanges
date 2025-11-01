using BaseStockConnectorInterface;
using StockConnector;

namespace BaseStockConnector
{
    public interface IStockHttp : IStockHttpConnector, IStockHttpRequest, IStockHttpOrder, IStockHttpTransfer, IRateLimitObservable
    {
    }
}
