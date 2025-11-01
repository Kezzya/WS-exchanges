using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Objects.Models.Socket;
using FameEX.Net.Objects.Socket.Subscriptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace FameEX.Net.Objects.Socket.Subscriptions
{
    internal class FameexTickerSubscription : FameexSubscription<FameexTicker>
    {
        private readonly Action<DataEvent<FameexTicker>> _typedHandler;

        public FameexTickerSubscription(ILogger logger, List<string> symbols, Action<DataEvent<FameexTicker>> handler)
            : base(logger, symbols, "ticker", $"market_{symbols[0].ToLower()}_ticker", handler, false)
        {
            _typedHandler = handler;
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var rawData = message.Data;
            if (rawData is string str)
            {
                try
                {
                    rawData = JsonConvert.DeserializeObject(str);
                }
                catch
                {
                    _logger.LogError("Failed to parse string as JSON");
                    return new CallResult(new ServerError("Invalid string format"));
                }
            }
            // Try to decompress if binary
            if (rawData is byte[] binaryData)
            {
                try
                {
                    // Check if it's gzip (magic bytes: 0x1f, 0x8b)
                    if (binaryData.Length > 2 && binaryData[0] == 0x1f && binaryData[1] == 0x8b)
                    {
                        using var memoryStream = new MemoryStream(binaryData);
                        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                        using var resultStream = new MemoryStream();
                        gzipStream.CopyTo(resultStream);
                        var decompressed = Encoding.UTF8.GetString(resultStream.ToArray());

                        _logger.LogDebug($"Decompressed ticker message: {decompressed}");
                        rawData = JsonConvert.DeserializeObject(decompressed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decompress Gzip data");
                    return new CallResult(new ServerError("Failed to decompress Gzip data"));
                }
            }

            // Parse the message
            if (rawData is Newtonsoft.Json.Linq.JToken token)
            {
                try
                {
                    var channel = token["channel"]?.ToString();
                    var tick = token["tick"];

                    _logger.LogDebug($"Processing ticker for channel: {channel}");

                    if (tick != null)
                    {
                        var ticker = tick.ToObject<FameexTicker>();
                        if (ticker != null)
                        {
                            _logger.LogDebug($"Ticker parsed: Close=${ticker.Close}, Volume={ticker.Volume}");
                            _typedHandler.Invoke(message.As(ticker));
                        }
                    }

                    return new CallResult(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse ticker message");
                    return new CallResult(new ServerError("Failed to parse ticker message"));
                }
            }

            _logger.LogError($"Invalid message format: expected JToken, got {rawData?.GetType().Name}");
            return new CallResult(new ServerError("Invalid message format"));
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(Newtonsoft.Json.Linq.JToken);
    }
}