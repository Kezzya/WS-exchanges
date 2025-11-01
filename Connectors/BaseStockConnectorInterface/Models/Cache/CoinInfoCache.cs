using System.Collections.Generic;
using System.Diagnostics;

namespace BaseStockConnectorInterface.Models.Cache
{
    /// <summary>
    /// Simple coin info object cache with Timer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CoinInfoCache<T>
    {
        public Stopwatch Timer { get; set; }

        public Dictionary<string, T> CoinInfoCached { get; set; }

        public CoinInfoCache(Dictionary<string, T> coinInfoDictionary)
        {
            CoinInfoCached = new Dictionary<string, T>(coinInfoDictionary);
            Timer = Stopwatch.StartNew();
        }
    }
}