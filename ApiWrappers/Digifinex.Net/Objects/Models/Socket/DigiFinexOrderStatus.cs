namespace Digifinex.Net.Objects.Models.Socket;

public enum DigifinexOrderStatus
{
    Unfilled = 0,
    PartiallyFilled = 1,
    FullyFilled = 2,
    CanceledUnfilled = 3,
    CanceledPartiallyFilled = 4
}