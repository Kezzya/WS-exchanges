using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexDepthSubscription : DigifinexSubscription<List<object>>
{
    private readonly Action<DataEvent<DigifinexDepthUpdate>> _typedHandler;

    public DigifinexDepthSubscription(ILogger logger, List<string> symbols, Action<DataEvent<DigifinexDepthUpdate>> handler)
        : base(logger, symbols, "depth", "depth.update", data => { }, false)
    {
        _typedHandler = handler;
    }

    public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
    {
        var data = (DigifinexSocketUpdate<List<object>>)message.Data;
        if (data.Params.Count >= 3)
        {
            var update = new DigifinexDepthUpdate
            {
                Clean = (bool)data.Params[0],
                Depth = ((Newtonsoft.Json.Linq.JObject)data.Params[1]).ToObject<DigifinexDepth>()!,
                Market = (string)data.Params[2]
            };
            _typedHandler.Invoke(message.As(update));
        }
        return new CallResult(null);
    }
}