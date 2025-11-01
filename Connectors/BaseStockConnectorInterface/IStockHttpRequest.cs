using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Instruments;
using BaseStockConnectorInterface.Models;
using BaseStockConnectorInterface.Models.Enums;
using BaseStockConnectorInterface.Models.Kline;
using BaseStockConnectorInterface.Models.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseStockConnectorInterface.Models.Ticker;
using BaseStockConnectorInterface.Models.Instruments;

namespace BaseStockConnector
{
    public interface IStockHttpRequest
    {
        /// <summary>
        /// Get Instrumets by Type
        /// </summary>
        /// <param name="instrumentType">Option or Futures</param>
        /// <returns></returns>
        public Task<List<BaseInstrumentModel>> GetInstrumetsAsync(InstrumentType instrumentType);

        /// <summary>
        /// Get depth order book
        /// </summary>
        /// <param name="instrumentType"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public Task<OrderBook> GetDepthAsync(InstrumentType instrumentType, string symbol, int depth);
        
        /// <summary>
        /// Get stock time
        /// </summary>
        /// <returns></returns>
        public Task<DateTime> GetServerTimestamp();

        /// <summary>
        /// Get coin info
        /// </summary>
        /// <returns></returns>
        public Task<List<SpotCoinModel>> GetCoinInfoAsync(InstrumentType instrumentType);

        /// <summary>
        /// Get coin info for specific instrument
        /// </summary>
        /// <param name="instrumentName"></param>
        /// <param name="instrumentType"></param>
        /// <returns></returns>
        public Task<SpotCoinModel> GetCoinInfoAsync(string instrumentName, InstrumentType instrumentType);

        /// <summary>
        /// Get Option List
        /// </summary>
        /// <param name="instrumentName"></param>
        /// <param name="expirationDate"></param>
        /// <returns></returns>
        public Task<ExchangeResponse<List<OptionInfo>>> GetOptionList(string? instrumentName, DateTime? expirationDate);

        public Task<ExchangeResponse<List<Volatility>>> GetVolatility(string instrumentName, DateTime from, DateTime to);
        /// <summary>
        /// Get stock Total balance
        /// </summary>
        /// <param name="coinName"></param>
        /// <returns></returns>
        public Task<BalancesModel> GetBalance(string coinName, BalanceType balance = BalanceType.Spot);
        /// <summary>
        /// Get stock main balance
        /// </summary>
        /// <param name="coinName"></param>
        /// <returns></returns>
        public Task<BalancesModel> GetBalanceMain(string coinName);
        
        public Task<Decimal> Get24hVolume(string coinName, InstrumentType instrumentType = InstrumentType.Spot);
        public Task<decimal> Get24hVolume(string coinName, DateTime date, InstrumentType instrumentType = InstrumentType.Spot);

        public Task<OrderInfo> GetLastTrade(string instrumentName, InstrumentType instrumentType = InstrumentType.Spot);

        public Task<BalanceTransfer> Transfer(AccountType from, AccountType to, string instrumentName, decimal amount);
        
        public Task<Withdraw> Withdrawal(string instrumentName, decimal amount, string address, string memo, string uniqueid, string network = "");
        
        public Task<WithdrawHistory> WithdrawalHistory(string id);

        public Task<List<KlineItem>> GetKlines(string instrumentName, KlineInterval klineInterval, DateTime? fromDateUtc, DateTime? toDateUtc, int? limit = null, InstrumentType instrumentType = InstrumentType.Spot);
    
        public Task<SocketListenKey> GetListenKey(InstrumentType instrumentType);

        public Task<SocketKeepAliveListenKey> KeepAlive(InstrumentType instrumentType, string listenKey);

        public Task<FundingRate?> GetFundingRate(string instrumentName);

        public Task<List<AssetBalance>> GetAllBalances(BalanceType balanceType);

        public Task<ExchangeResponse<List<MarketTickerModel>>> FetchTickersAsync();

        public Task<ExchangeResponse<OptionTickerModel>> GetOptionTicker(string instrumentName);

        public Task<Commission?> GetAccountCommissionAsync(string instrumentName, InstrumentType instrumentType);

    }
}
