using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Models;
using FameEX.Net.Objects.Models.Socket;
using FameEX.Net.Objects.Socket.Subscriptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexOrderSubscription : FameexSubscription<FameexOrderUpdate>
    {
        private readonly Action<DataEvent<FameexOrderUpdate>> _typedHandler;
        private readonly List<string> _symbols;
        public FameexOrderSubscription(ILogger logger, Action<DataEvent<FameexOrderUpdate>> handler)
            : base(logger, new List<string>(), "order", "order", handler, true)
        {
            _typedHandler = handler;
            _symbols = new List<string>();
        }
        public FameexOrderSubscription(ILogger logger, Action<DataEvent<FameexOrderUpdate>> handler, string symbol)
    : base(logger, new List<string> { symbol }, "spot.orders", "sub", handler, true)
        {
            _typedHandler = handler;
            _symbols = new List<string> { symbol };
        }
        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var rawData = message.Data;

            if (rawData is byte[] binaryData)
            {
                try
                {
                    if (binaryData.Length > 2 && binaryData[0] == 0x1F && binaryData[1] == 0x8B)
                    {
                        using var memoryStream = new MemoryStream(binaryData);
                        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                        using var resultStream = new MemoryStream();
                        gzipStream.CopyTo(resultStream);
                        var decompressed = Encoding.UTF8.GetString(resultStream.ToArray());

                        _logger.LogDebug($"Decompressed order update message: {decompressed}");
                        rawData = JsonConvert.DeserializeObject(decompressed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decompress Gzip data");
                    return new CallResult(new ServerError("Failed to decompress Gzip data"));
                }
            }

            if (rawData is JToken token)
            {
                try
                {
                    var channel = token["channel"]?.ToString();
                    var tick = token["tick"];

                    _logger.LogDebug($"Processing order update for channel: {channel}");

                    if (tick != null)
                    {
                        var orderUpdate = JsonConvert.DeserializeObject<FameexOrderUpdate>(tick.ToString());
                        if (orderUpdate == null)
                        {
                            _logger.LogError("Invalid order update format");
                            return new CallResult(new ServerError("Invalid order update format"));
                        }
                        if (_symbols.Any() && orderUpdate.Symbol != _symbols.First())
                            return new CallResult(null);

                        _logger.LogDebug($"Parsed order update: {JsonConvert.SerializeObject(orderUpdate)}");
                        _typedHandler.Invoke(message.As(orderUpdate));
                    }

                    return new CallResult(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse order update message");
                    return new CallResult(new ServerError("Failed to parse order update message"));
                }
            }

            _logger.LogError($"Invalid message format: expected JToken, got {rawData?.GetType().Name}");
            return new CallResult(new ServerError("Invalid message format"));
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(JToken);
    }
}
