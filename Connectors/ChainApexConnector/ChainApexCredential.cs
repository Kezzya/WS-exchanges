using StockConnector;

namespace ChainApexConnector
{
    public class ChainApexCredential : IStockCredential
    {
        public string ApiKey { get; set; }

        public string Secret { get; set; }
    }
}
