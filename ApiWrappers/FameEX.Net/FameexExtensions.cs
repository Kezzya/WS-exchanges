using BaseStockConnectorInterface.Models.Kline;
using Fameex.Net.Objects.Models.ExchangeData;
using FameEX.Net.Objects.Models;

namespace FameexConnector
{
    public static class FameexExtensions
    {
        public static FameexKlineInterval ToFameexKlineInterval(this KlineInterval interval)
        {
            return interval switch
            {
                KlineInterval.OneMinute => FameexKlineInterval.OneMinute,
                KlineInterval.FiveMinutes => FameexKlineInterval.FiveMinutes,
                KlineInterval.FifteenMinutes => FameexKlineInterval.FifteenMinutes,
                KlineInterval.ThirtyMinutes => FameexKlineInterval.ThirtyMinutes,
                KlineInterval.OneHour => FameexKlineInterval.OneHour,
                KlineInterval.FourHour => FameexKlineInterval.FourHours,
                KlineInterval.OneDay => FameexKlineInterval.OneDay,
                KlineInterval.OneMonth => FameexKlineInterval.OneMonth,
                _ => throw new ArgumentException($"Unsupported kline interval: {interval}")
            };
        }

        public static TimeSpan ToTimeSpan(this FameexKlineInterval interval)
        {
            return interval switch
            {
                FameexKlineInterval.OneMinute => TimeSpan.FromMinutes(1),
                FameexKlineInterval.FiveMinutes => TimeSpan.FromMinutes(5),
                FameexKlineInterval.FifteenMinutes => TimeSpan.FromMinutes(15),
                FameexKlineInterval.ThirtyMinutes => TimeSpan.FromMinutes(30),
                FameexKlineInterval.OneHour => TimeSpan.FromHours(1),
                FameexKlineInterval.FourHours => TimeSpan.FromHours(4),
                FameexKlineInterval.TwelveHours => TimeSpan.FromHours(12),
                FameexKlineInterval.OneDay => TimeSpan.FromDays(1),
                FameexKlineInterval.OneWeek => TimeSpan.FromDays(7),
                FameexKlineInterval.OneMonth => TimeSpan.FromDays(30),
                _ => throw new ArgumentException($"Unknown interval: {interval}")
            };
        }
    }
}