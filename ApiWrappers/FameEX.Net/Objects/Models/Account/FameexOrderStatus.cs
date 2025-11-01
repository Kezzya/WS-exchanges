using CryptoExchange.Net.Attributes;

namespace FameEX.Net.Objects.Models.Account
{
    public enum FameexOrderStatus
    {
        [Map("New Order")]
        NewOrder,

        [Map("Partially Filled")]
        PartiallyFilled,

        [Map("Filled")]
        Filled,

        [Map("Canceled")]
        Canceled,

        [Map("PENDING_CANCEL")]
        PendingCancel,

        [Map("Rejected")]
        Rejected,

        [Map("NEW_")]     // из openOrders
        NewUnderscore,

        [Map("NEW")]      // из order response
        New,

        [Map("Expired")]
        Expired,

        [Map("Insurance")]
        Insurance,

        [Map("ADL")]
        Adl
    }
}