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
    internal class FameexTradesSubscription : FameexSubscription<List<FameexTrade>>
    {
        private readonly Action<DataEvent<List<FameexTrade>>> _typedHandler;

        public FameexTradesSubscription(ILogger logger, List<string> symbols, Action<DataEvent<List<FameexTrade>>> handler)
            : base(logger, symbols, "trade_ticker", $"market_{symbols[0].ToLower()}_trade_ticker", handler, false)
        {
            _typedHandler = handler;
        }

        public override CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message)
        {
            var rawData = message.Data;

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

                        _logger.LogDebug($"Decompressed trades message: {decompressed}");
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

                    _logger.LogDebug($"Processing trades for channel: {channel}");

                    if (tick != null)
                    {
                        var data = tick["data"];
                        if (data != null)
                        {
                            var trades = data.ToObject<List<FameexTrade>>();
                            if (trades != null && trades.Count > 0)
                            {
                                _logger.LogDebug($"Parsed {trades.Count} trades");
                                _typedHandler.Invoke(message.As(trades));
                            }
                        }
                    }

                    return new CallResult(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse trades message");
                    return new CallResult(new ServerError("Failed to parse trades message"));
                }
            }

            _logger.LogError($"Invalid message format: expected JToken, got {rawData?.GetType().Name}");
            return new CallResult(new ServerError("Invalid message format"));
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(Newtonsoft.Json.Linq.JToken);
    }
}