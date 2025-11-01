using BaseStockConnectorInterface.Models.Kline;
using Digifinex.Net.Objects.Models.Account;
using Digifinex.Net.Objects.Models.ExchangeData;

namespace DigifinexConnector
{
    internal static class DigifinexConnectorExtensions
    {
        public static DigifinexKlineInterval ToDigifinexKlineInterval(this KlineInterval klineInterval)
        {
            switch (klineInterval)
            {
                case KlineInterval.OneMinute:
                    return DigifinexKlineInterval.OneMinute;
                case KlineInterval.FiveMinutes:
                    return DigifinexKlineInterval.FiveMinutes;
                case KlineInterval.FifteenMinutes:
                    return DigifinexKlineInterval.FifteenMinutes;
                case KlineInterval.ThirtyMinutes:
                    return DigifinexKlineInterval.ThirtyMinutes;
                case KlineInterval.OneHour:
                    return DigifinexKlineInterval.OneHour;
                case KlineInterval.FourHour:
                    return DigifinexKlineInterval.FourHours;
                case KlineInterval.OneDay:
                    return DigifinexKlineInterval.OneDay;
                default:
                    throw new Exception("connector converter error");
            }
        }

        public static TransactionStatus ToDepositStatus(this DigifinexDepositState status)
        {
            return status switch
            {
                DigifinexDepositState.InDeposit => TransactionStatus.Pending,
                DigifinexDepositState.ToBeConfirmed => TransactionStatus.Pending,
                DigifinexDepositState.SuccessfullyDeposited => TransactionStatus.Ok,
                DigifinexDepositState.Stopped => TransactionStatus.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static TransactionStatus ToWithdrawStatus(this DigifinexWithdrawState status)
        {
            return status switch
            {
                DigifinexWithdrawState.ApplicationInProgress => TransactionStatus.Pending,
                DigifinexWithdrawState.ToBeConfirmed => TransactionStatus.Pending,
                DigifinexWithdrawState.Completed => TransactionStatus.Ok,
                DigifinexWithdrawState.Rejected => TransactionStatus.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}