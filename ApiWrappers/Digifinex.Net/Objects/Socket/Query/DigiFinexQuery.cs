using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;

namespace Digifinex.Net.Objects.Socket.Query
{
    internal class DigifinexQuery<T> : Query<DigifinexSocketResponse<T>>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public DigifinexQuery(string method, object[] parameters, bool authenticated = false, int weight = 1)
            : base(new DigifinexSocketRequest { Id = NextId(), Method = method, Params = parameters }, authenticated, weight)
        {
            ListenerIdentifiers = new HashSet<string> { ((DigifinexSocketRequest)Request).Id.ToString() };
        }

        public override CallResult<DigifinexSocketResponse<T>> HandleMessage(SocketConnection connection, DataEvent<DigifinexSocketResponse<T>> message)
        {
            if (message.Data.Error != null)
                return new CallResult<DigifinexSocketResponse<T>>(new ServerError(message.Data.Error.ToString()));

            return new CallResult<DigifinexSocketResponse<T>>(message.Data);
        }

        private static int _nextId = 1;
        private static int NextId() => System.Threading.Interlocked.Increment(ref _nextId);
    }
}