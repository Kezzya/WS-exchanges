using System.Collections.Concurrent;
using BaseStockConnector;
using BaseStockConnectorInterface;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace DigifinexConnector
{
    public class DigifinexConnector : IStock<CallResult<UpdateSubscription>>
    {
        public string StockName { get; }

        public IStockHttp HttpClient { get; set; }

        public IStockSocket<CallResult<UpdateSubscription>> SocketClient { get; set; }

        private static ConcurrentDictionary<string, string> _internalOrderIdsDictionary = new ConcurrentDictionary<string, string>();


        public static string SymbolSeparator { get; set; } = "_";

        public DigifinexConnector()
        {
            StockName = "Digifinex";
            SocketClient = new DigifinexSocketConnector(this, _internalOrderIdsDictionary);;
            HttpClient = new DigifinexHttpConnector((DigifinexSocketConnector)SocketClient, _internalOrderIdsDictionary);
        }

        public static string GetInstrumentName(BaseStockConnector.Models.Enums.InstrumentType instrumentType, string baseAsset, string quoteAsset)
        {
            return $"{baseAsset}{SymbolSeparator}{quoteAsset}".ToLower();
        }
    }
}