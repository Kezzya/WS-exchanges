namespace BaseStockConnectorInterface.Models.Kline
{
    public enum KlineInterval
    {
        OneMinute = 60,
        FiveMinutes = 60 * 5,
        FifteenMinutes = 60 * 15,
        ThirtyMinutes = 60 * 30,
        OneHour = 60 * 60,
        FourHour = 60 * 60 * 4,
        OneDay = 60 * 60 * 24,
        OneMonth = 60 * 60 * 24 * 30
    }
}
