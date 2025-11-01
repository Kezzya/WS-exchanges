using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Objects.Socket.Subscriptions;

internal class DigifinexTradesSubscription : DigifinexSubscription<List<object>>
{
    private readonly Action<DataEvent<DigifinexTradesUpdate>> _typedHandler;

    public DigifinexTradesSubscription(ILogger logger, List<string> symbols, Action<DataEvent<DigifinexTradesUpdate>> handler)
        : base(logger, symbols, "trades", "trades.update", data => { }, false)
    {
        _typedHandler = handler;
    }

    public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
    {
        var data = (DigifinexSocketUpdate<List<object>>)message.Data;
        if (data.Params.Count >= 3)
        {
            var update = new DigifinexTradesUpdate
            {
                Clean = (bool)data.Params[0],
                Trades = ((Newtonsoft.Json.Linq.JArray)data.Params[1]).ToObject<List<DigifinexTrade>>()!,
                Market = (string)data.Params[2]
            };
            _typedHandler.Invoke(message.As(update));
        }
        return new CallResult(null);
    }
}