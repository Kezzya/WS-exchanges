using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;

namespace Digifinex.Net.Objects.Socket.Query;

internal class DigifinexPingQuery : Query<DigifinexSocketResponse<string>>
{
    public override HashSet<string> ListenerIdentifiers { get; set; }

    public DigifinexPingQuery() : base(new DigifinexSocketRequest { Id = NextId(), Method = "server.ping", Params = new object[0] }, false)
    {
        ListenerIdentifiers = new HashSet<string> { ((DigifinexSocketRequest)Request).Id.ToString() };
    }

    public override CallResult<DigifinexSocketResponse<string>> HandleMessage(SocketConnection connection, DataEvent<DigifinexSocketResponse<string>> message)
    {
        return new CallResult<DigifinexSocketResponse<string>>(message.Data);
    }

    private static int _nextId = 1;
    private static int NextId() => System.Threading.Interlocked.Increment(ref _nextId);
}