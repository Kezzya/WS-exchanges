namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexTradesUpdate
{
    public bool Clean { get; set; }
    public List<DigifinexTrade> Trades { get; set; } = new List<DigifinexTrade>();
    public string Market { get; set; } = string.Empty;
}