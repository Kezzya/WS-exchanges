public enum TransferAccountType
{
    Funding, // In some exchanges, funding and spot are the same account
    Main,    // For exchanges that allow subaccounts
    Spot,
    Margin,
    Future,
    Swap,
    Lending
}