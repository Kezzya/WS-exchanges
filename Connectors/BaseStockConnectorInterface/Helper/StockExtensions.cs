using System;

namespace BaseStockConnectorInterface.Helper
{
    public static class StockExtensions
    {
        public static (string BaseAsset, string QuoteAsset) SplitSymbol(string symbol, string separator)
        {
            var splitArray = symbol.Split(separator);
            if (splitArray.Length != 2)
            {
                throw new ArgumentException($"The string {symbol} is not the symbol or can't be separate with {separator}");
            }

            return (splitArray[0], splitArray[1]);
        }
    }
}
