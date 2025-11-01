using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;

namespace ChainApex.Net.Objects.Models.Socket
{
    internal class ChainApexQuery : Query<ChainApexPingPong>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public ChainApexQuery(ChainApexSocketRequest request, string channel, int weight = 1)
            : base(request, false, weight)
        {
            ListenerIdentifiers = new HashSet<string> { channel };
        }

        public override CallResult<ChainApexPingPong> HandleMessage(SocketConnection connection, DataEvent<ChainApexPingPong> message)
        {
            // ChainApex doesn't send acknowledgment messages for subscriptions
            // Success is indicated by receiving data on the channel
            return new CallResult<ChainApexPingPong>(message.Data);
        }
    }
}

