using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Objects.Socket;
using FameEX.Net.Objects.Models.Socket;

namespace FameEX.Net.Objects.Socket.Query
{
    internal class FameexAuthQuery : Query<FameexSocketResponse<FameexAuthResult>>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        // ✅ ИСПРАВЛЕНО: Убрали timestamp и sign - они не нужны!
        public FameexAuthQuery(string apiKey)
            : base(new FameexSocketRequest
            {
                Method = "server.auth",
                // Только apiKey, без timestamp и signature
                Params = new object[] { apiKey }
            }, true)
        {
            ListenerIdentifiers = new HashSet<string> { ((FameexSocketRequest)Request).Id.ToString() };
        }

        public override CallResult<FameexSocketResponse<FameexAuthResult>> HandleMessage(
            SocketConnection connection,
            DataEvent<FameexSocketResponse<FameexAuthResult>> message)
        {
            if (message.Data.Error != null)
                return new CallResult<FameexSocketResponse<FameexAuthResult>>(
                    new ServerError(message.Data.Error.ToString()));

            return new CallResult<FameexSocketResponse<FameexAuthResult>>(message.Data);
        }

        private static int _nextId = 1;
        private static int NextId() => System.Threading.Interlocked.Increment(ref _nextId);
    }

}