using Newtonsoft.Json;

namespace Digifinex.Net.Objects.Models.ExchangeData;

public class DigifinexCurrency
{
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("chain")]
    public string Chain { get; set; } = string.Empty;

    [JsonProperty("min_deposit_amount")]
    public decimal MinDepositAmount { get; set; }

    [JsonProperty("min_withdraw_amount")]
    public decimal MinWithdrawAmount { get; set; }

    [JsonProperty("deposit_status")]
    public int DepositStatus { get; set; }

    [JsonProperty("withdraw_status")]
    public int WithdrawStatus { get; set; }

    [JsonProperty("withdraw_fee_currency")]
    public string WithdrawFeeCurrency { get; set; } = string.Empty;

    [JsonProperty("min_withdraw_fee")]
    public decimal MinWithdrawFee { get; set; }

    [JsonProperty("withdraw_fee_rate")]
    public decimal WithdrawFeeRate { get; set; }
}