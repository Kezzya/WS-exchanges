using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Objects.Socket;

namespace FameEX.Net.Objects.Socket.Query
{
    internal class FameexPingQuery : Query<FameexSocketResponse<string>>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public FameexPingQuery() : base(new FameexSocketRequest { Id = NextId(), Method = "server.ping", Params = new object[0] }, false)
        {
            ListenerIdentifiers = new HashSet<string> { ((FameexSocketRequest)Request).Id.ToString() };
        }

        public override CallResult<FameexSocketResponse<string>> HandleMessage(SocketConnection connection, DataEvent<FameexSocketResponse<string>> message)
        {
            return new CallResult<FameexSocketResponse<string>>(message.Data);
        }

        private static int _nextId = 1;
        private static int NextId() => System.Threading.Interlocked.Increment(ref _nextId);
    }
}