namespace ChainApex.Net.Objects.Models.Account
{
    public enum ChainApexOrderStatus
    {
        New,
        PartiallyFilled,
        Filled,
        Cancelled,
        ToBeCancelled,
        PartiallyFilledCancelled,
        Rejected
    }
}
