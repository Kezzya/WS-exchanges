using System;
using BaseStockConnector;

namespace BaseStockConnectorInterface
{
    public interface IStockSocket<T>: IStockSocketConnector, IStockSocketSubscribe<T>, IDisposable
    {
        /// <summary>
        /// For socket connection identification
        /// </summary>
        string ApiKey { get; }
    }
}
