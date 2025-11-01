namespace Digifinex.Net.Objects
{
    internal class OurbitApiAddresses
    {
        public string RestClientAddress { get; set; } = "";
        public string PublicSocketClientSpotAddress { get; set; }

        public static OurbitApiAddresses Default = new OurbitApiAddresses
        {
            RestClientAddress = "https://api.ourbit.com",
            PublicSocketClientSpotAddress = "wss://wbs.ourbit.com/ws"
        };
    }
}