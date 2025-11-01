using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseStockConnectorInterface.Models.Kline
{
    /*
     * converting is needed for:
     * ==kukoin
     * ----1 month
     * ==p2pb2b all except m1,h1,d1
     */
    public static class KlineIntervalDataConverter
    {
        public static List<KlineItem> ConvertInterval(List<KlineItem> inputItems, KlineInterval inputInterval, KlineInterval outputInterval)
        {
            if (inputInterval > outputInterval) throw new Exception("ConvertInterval() error. converting is posible only to bigger interval");
            if (inputInterval == outputInterval) return inputItems;

            Func<KlineItem, DateTime> outputIntervalGroupPredicate;
            switch (outputInterval)
            {
                case KlineInterval.FiveMinutes:
                    outputIntervalGroupPredicate = (i) => MinutesPredicate(5, i);
                    break;
                case KlineInterval.FifteenMinutes:
                    outputIntervalGroupPredicate = (i) => MinutesPredicate(15, i);
                    break;
                case KlineInterval.ThirtyMinutes:
                    outputIntervalGroupPredicate = (i) => MinutesPredicate(30, i);
                    break;
                case KlineInterval.FourHour:
                    outputIntervalGroupPredicate = (i) => HoursPredicate(4, i);
                    break;
                case KlineInterval.OneMonth:
                    outputIntervalGroupPredicate = (i) => OneMonthPredicate(i);
                    break;
                default:
                    throw new Exception("ConvertInterval() error. unknown interval predicate for groupBy");
            }

            var outputGroups = inputItems.GroupBy(outputIntervalGroupPredicate).ToList();
            var resultOutputItems = new List<KlineItem>();
            foreach (var outputGoup in outputGroups)
            {
                var monthGoupSorted = outputGoup.OrderBy(i => i.OpenTime).ToList();
                var newOutputItem = new KlineItem()
                {
                    OpenTime = outputGoup.Key,
                    LowPrice = monthGoupSorted.Min(i => i.LowPrice),
                    HighPrice = monthGoupSorted.Max(i => i.HighPrice),
                    OpenPrice = monthGoupSorted.First().OpenPrice,
                    ClosePrice = monthGoupSorted.Last().ClosePrice,
                    Volume = monthGoupSorted.Sum(i => i.Volume),
                };
                resultOutputItems.Add(newOutputItem);
            }
            return resultOutputItems;
        }

        private static DateTime MinutesPredicate(int minutes, KlineItem i)
        {
            var groupIndex = i.OpenTime.Minute / minutes;
            return new DateTime(i.OpenTime.Year, i.OpenTime.Month, i.OpenTime.Day, i.OpenTime.Hour, groupIndex * minutes, 0);
        }

        private static DateTime HoursPredicate(int hours, KlineItem i)
        {
            var groupIndex = i.OpenTime.Hour / hours;
            return new DateTime(i.OpenTime.Year, i.OpenTime.Month, i.OpenTime.Day, groupIndex * hours, 0, 0);
        }

        private static DateTime OneMonthPredicate(KlineItem i)
        {
            return new DateTime(i.OpenTime.Year, i.OpenTime.Month, 1);
        }
    }
}
