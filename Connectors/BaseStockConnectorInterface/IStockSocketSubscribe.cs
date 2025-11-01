using BaseStockConnector.Models;
using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface.Models.Instruments;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseStockConnector
{
    public interface IStockSocketSubscribe<T>
    {
        /// <summary>
        /// Subscribe to stock option ticker update
        /// </summary>
        /// <param name="instrumentNames">List of subscribed instrument names</param>
        /// <param name="onMessage">Call back on message</param>
        /// <returns>Subscription result</returns>
        //public Task<SubscriptionResult<T>> OptionTikerUpdateSubscribeAsync(List<string> instrumentNames, Action<OptionInstrumentDataModel> onMessage);
        //public Task<SubscriptionResult<T>> OptionTikerUpdateSubscribeAsync(string instrumentName, Action<OptionInstrumentDataModel> onMessage);

        public Task<SubscriptionResult<T>> PartialOrderBookSubscribeAsync(string instrumentName, InstrumentType instrumentType, int? level, Action<OrderBook> onMessage);
        public Task<SubscriptionResult<T>> IndexTickerSubscribeAsync(string instrumentName, InstrumentType instrumentType, Action<TickerUpdateModel> onMessage);
        public Task<SubscriptionResult<T>> PriceUpdateSubscribeAsync(string instrumentName, InstrumentType instrumentType, Action<PriceUpdateModel> onMessage);
        /// <summary>
        /// IMPORTANT: The status must match the filled volume. The filled volume and the total volume are in the base currency!
        /// </summary>
        /// <param name="instrumentType"></param>
        /// <param name="instrumentNames"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public Task<SubscriptionResult<T>> OrderUpdateSubscribeAsync(InstrumentType instrumentType, List<string> instrumentNames, Action<OrderUpdate> onMessage);

    }
}
