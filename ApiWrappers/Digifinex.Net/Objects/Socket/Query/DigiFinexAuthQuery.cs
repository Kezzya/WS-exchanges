using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;

namespace Digifinex.Net.Objects.Socket.Query;

internal class DigifinexAuthQuery : Query<DigifinexSocketResponse<DigifinexAuthResult>>
{
    public override HashSet<string> ListenerIdentifiers { get; set; }

    public DigifinexAuthQuery(string apiKey, string timestamp, string sign)
        : base(new DigifinexSocketRequest 
        { 
            Id = NextId(), 
            Method = "server.auth", 
            Params = new object[] { apiKey, timestamp, sign } 
        }, true)
    {
        ListenerIdentifiers = new HashSet<string> { ((DigifinexSocketRequest)Request).Id.ToString() };
    }

    public override CallResult<DigifinexSocketResponse<DigifinexAuthResult>> HandleMessage(
        SocketConnection connection, 
        DataEvent<DigifinexSocketResponse<DigifinexAuthResult>> message)
    {
        if (message.Data.Error != null)
            return new CallResult<DigifinexSocketResponse<DigifinexAuthResult>>(new ServerError(message.Data.Error.ToString()));

        return new CallResult<DigifinexSocketResponse<DigifinexAuthResult>>(message.Data);
    }

    private static int _nextId = 1;
    private static int NextId() => System.Threading.Interlocked.Increment(ref _nextId);
}