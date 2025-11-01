namespace Digifinex.Net.Objects.Models.Socket;

public class DigifinexDepthUpdate
{
    public bool Clean { get; set; }
    public DigifinexDepth Depth { get; set; } = new DigifinexDepth();
    public string Market { get; set; } = string.Empty;
}