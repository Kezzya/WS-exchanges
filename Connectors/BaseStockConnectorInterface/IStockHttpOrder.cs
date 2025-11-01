using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseStockConnectorInterface.Models.Enums;

namespace BaseStockConnector
{
    public interface IStockHttpOrder
    {
        /// <summary>
        /// make order
        /// </summary>
        /// <param name="instrumentType">Option or Futures</param>
        /// <param name="orderType">Market or Limit</param>
        /// <param name="direction">Buy Sell</param>
        /// <param name="instrument">name instrument</param>
        /// <param name="volume">volume or quantity</param>
        /// <param name="price">Price, just need for LIMIT ordertype</param>
        /// <returns></returns>
        public Task<BaseOrderModel> MakeOrderAsync(InstrumentType instrumentType, OrderType orderType, Direction direction, PositionOrderType positionOrderType, string instrument, decimal volume, decimal? price, string orderId);
        public Task<IList<BaseOrderModel>> MakeBatchOrdersAsync(InstrumentType instrumentType, IList<OrderCreateRequest> orders);
        public Task<BaseOrderModel> CancelOrderAsync(InstrumentType instrumentType, string instrumentName, string orderId);
        public Task<BaseOrderModel> ClosePositionAsync(string instrumentName, string orderId);
        public Task<List<BaseOrderModel>> GetActiveOrdersAsync(string instrumentName, InstrumentType instrumentType);
        public Task<bool> SetLeverage(string instrumentName, int leverage);
        public Task<bool> SetLeverage(List<string> instrumentNames, int leverage);
        public Task<List<OrderHistoryItem>> GetOrderHistory(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType = InstrumentType.Spot);
        public Task<List<OrderHistoryItem>> GetOrderHistoryForPeriodWithFee(string instrumentName, DateTime fromDateUtc, DateTime toDateUtc, InstrumentType instrumentType = InstrumentType.Spot);
        public Task<OrderSyncInfo?> GetOrderInfo(string instrumentName, string orderId, InstrumentType instrumentType = InstrumentType.Spot);
        /// <summary>
        /// Get orders trades history
        /// </summary>
        /// <param name="instrumentName"></param>
        /// <param name="orderIds">List needed orders ids</param>
        /// <param name="fromDateUtc"></param>
        /// <param name="toDateUtc"></param>
        /// <returns>Dictonary<OrderId, OrderTrades></returns>
        public Task<Dictionary<string, List<OrderTrade>>> GetTradeHistoryByOrders(string instrumentName, List<string> orderIds, DateTime fromDateUtc, DateTime toDateUtc);
        public Task<List<OrderTrade>> GetTradeHistory(InstrumentType instrumentType, string instrumentName, DateTime fromDateUtc, DateTime toDateUtc);
        public Task<List<Position>> GetPositionsAsync(string currency, InstrumentType instrumentType = InstrumentType.Option);
    }
}
