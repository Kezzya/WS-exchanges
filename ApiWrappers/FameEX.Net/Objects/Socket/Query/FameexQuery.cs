using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Objects.Models.Socket;

namespace FameEX.Net.Objects.Socket.Query
{
    internal class FameexQuery<T> : Query<FameexSocketResponse<T>>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public FameexQuery(string method, object[] parameters, bool authenticated = false, int weight = 1)
            : base(new FameexSocketRequest { Id = NextId(), Method = method, Params = parameters }, authenticated, weight)
        {
            ListenerIdentifiers = new HashSet<string> { ((FameexSocketRequest)Request).Id.ToString() };
        }

        public override CallResult<FameexSocketResponse<T>> HandleMessage(SocketConnection connection, DataEvent<FameexSocketResponse<T>> message)
        {
            if (message.Data.Error != null)
                return new CallResult<FameexSocketResponse<T>>(new ServerError(message.Data.Error.ToString()));

            return new CallResult<FameexSocketResponse<T>>(message.Data);
        }

        private static int _nextId = 1;
        private static int NextId() => System.Threading.Interlocked.Increment(ref _nextId);
    }
}