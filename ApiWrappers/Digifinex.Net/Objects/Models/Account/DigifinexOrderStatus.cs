namespace Digifinex.Net.Objects.Models.Account;

public enum DigifinexOrderStatus
{
    NoneExecuted = 0,
    PartiallyExecuted = 1,
    FullyExecuted = 2,
    CancelledNoneExecuted = 3,
    CancelledPartiallyExecuted = 4
}