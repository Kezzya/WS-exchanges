using StockConnector;

namespace DigifinexConnector
{
    public class DigifinexCredential : IStockCredential
    {
        public string ApiKey { get; set; }

        public string Secret { get; set; }
    }
}