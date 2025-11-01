using BaseStockConnectorInterface.Models.Kline;
using FameEX.Net.Models;

namespace FameexConnector
{
    internal static class FameexConnectorExtensions
    {
        public static string ToFameexKlineInterval(this KlineInterval klineInterval)
        {
            return klineInterval switch
            {
                KlineInterval.OneMinute => "1m",
                KlineInterval.FiveMinutes => "5m",
                KlineInterval.FifteenMinutes => "15m",
                KlineInterval.ThirtyMinutes => "30m",
                KlineInterval.OneHour => "1h",
                KlineInterval.FourHour => "4h",
                KlineInterval.OneDay => "1d",
                _ => throw new Exception("Unsupported kline interval")
            };
        }

        public static TransactionStatus ToDepositStatus(this string status)
        {
            return status.ToLower() switch
            {
                "pending" => TransactionStatus.Pending,
                "confirming" => TransactionStatus.Pending,
                "success" => TransactionStatus.Ok,
                "failed" => TransactionStatus.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static TransactionStatus ToWithdrawStatus(this string status)
        {
            return status.ToLower() switch
            {
                "pending" => TransactionStatus.Pending,
                "processing" => TransactionStatus.Pending,
                "completed" => TransactionStatus.Ok,
                "rejected" => TransactionStatus.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}