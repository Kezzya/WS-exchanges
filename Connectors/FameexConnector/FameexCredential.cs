using StockConnector;

namespace FameexConnector
{
    public class FameexCredential : IStockCredential
    {
        public string ApiKey { get; set; }

        public string Secret { get; set; }
    }
}