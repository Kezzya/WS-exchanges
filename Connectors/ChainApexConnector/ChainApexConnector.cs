using System.Collections.Concurrent;
using BaseStockConnector;
using BaseStockConnector.Models.Enums;
using BaseStockConnectorInterface;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace ChainApexConnector
{
    public class ChainApexConnector : IStock<CallResult<UpdateSubscription>>
    {
        public string StockName { get; }

        public IStockHttp HttpClient { get; set; }

        public IStockSocket<CallResult<UpdateSubscription>> SocketClient { get; set; }

        private static ConcurrentDictionary<string, string> _internalOrderIdsDictionary = new ConcurrentDictionary<string, string>();

        public static string SymbolSeparator { get; set; } = "";

        public ChainApexConnector()
        {
            StockName = "ChainApex";
            var socketConnector = new ChainApexSocketConnector(this, _internalOrderIdsDictionary);
            SocketClient = socketConnector;
            HttpClient = new SimpleChainApexHttpConnector(socketConnector, _internalOrderIdsDictionary);
        }

        public ChainApexConnector(string? customRestUrl, string? customWebSocketUrl)
        {
            StockName = "ChainApex";
            var socketConnector = new ChainApexSocketConnector(this, _internalOrderIdsDictionary);
            SocketClient = socketConnector;
            var httpConnector = new SimpleChainApexHttpConnector(socketConnector, _internalOrderIdsDictionary);
            httpConnector.SetCustomUrls(customRestUrl, customWebSocketUrl);
            HttpClient = httpConnector;
        }

        public static string GetInstrumentName(InstrumentType instrumentType, string baseAsset, string quoteAsset)
        {
            return $"{baseAsset}{SymbolSeparator}{quoteAsset}".ToUpper();
        }
    }
}
