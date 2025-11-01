using System.Collections.Concurrent;
using BaseStockConnector;
using BaseStockConnectorInterface;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace FameexConnector
{
    public class FameexConnector : IStock<CallResult<UpdateSubscription>>
    {
        public string StockName { get; }

        public IStockHttp HttpClient { get; set; }

        public IStockSocket<CallResult<UpdateSubscription>> SocketClient { get; set; }

        private static ConcurrentDictionary<string, string> _internalOrderIdsDictionary = new ConcurrentDictionary<string, string>();

        public static string SymbolSeparator { get; set; } = "";

        public FameexConnector()
        {
            StockName = "FameEX";
            SocketClient = new FameexSocketConnector(this, _internalOrderIdsDictionary);
            HttpClient = new FameexHttpConnector((FameexSocketConnector)SocketClient, _internalOrderIdsDictionary);
        }

        public static string GetInstrumentName(BaseStockConnector.Models.Enums.InstrumentType instrumentType, string baseAsset, string quoteAsset)
        {
            return $"{baseAsset}{quoteAsset}".ToUpper();
        }
    }
}