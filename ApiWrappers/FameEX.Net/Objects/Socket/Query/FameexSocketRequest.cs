using CryptoExchange.Net.Objects;

namespace FameEX.Net.Objects.Socket
{
    public class FameexSocketRequest
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public object[] Params { get; set; }
    }
}