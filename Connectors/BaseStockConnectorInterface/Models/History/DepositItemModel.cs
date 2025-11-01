using System;

public class DepositItemModel
{
    public object Info { get; set; }

    public string Id { get; set; }

    public string Txid { get; set; }

    public long Timestamp { get; set; }

    public DateTime DateTime { get; set; }

    public string AddressFrom { get; set; }

    public string Address { get; set; }

    public string AddressTo { get; set; }

    public string TagFrom { get; set; }

    public string Tag { get; set; }

    public string TagTo { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public TransactionStatus Status { get; set; }

    public long? Updated { get; set; } // Nullable for undefined values

    public string Comment { get; set; }

    public FeeModel? Fee { get; set; }
}

public class FeeModel
{
    public string Currency { get; set; }

    public decimal? Cost { get; set; }

    public decimal? Rate { get; set; } 
}