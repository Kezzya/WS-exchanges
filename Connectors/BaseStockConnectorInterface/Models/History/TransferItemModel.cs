using System;

public class TransferItemModel
{
    public string Id { get; set; }

    public long Timestamp { get; set; }

    public DateTime DateTime { get; set; }

    public string Currency { get; set; }

    public decimal Amount { get; set; }

    public string FromAccount { get; set; } // Can be exchange-specific or unified

    public string ToAccount { get; set; }   // Can be exchange-specific or unified

    public TransferStatus Status { get; set; }
}