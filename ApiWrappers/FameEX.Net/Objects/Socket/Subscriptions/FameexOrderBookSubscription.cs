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
    internal class FameexOrderBookSubscription : FameexSubscription<FameexOrderBook>
    {
        private readonly Action<DataEvent<FameexOrderBook>> _typedHandler;

        public FameexOrderBookSubscription(ILogger logger, List<string> symbols, int depth, Action<DataEvent<FameexOrderBook>> handler)
            : base(logger, symbols, "depth", $"market_{symbols[0].ToLower()}_depth_step0", handler, false)
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

                        _logger.LogDebug($"Decompressed order book message: {decompressed}");
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
            if (rawData is JToken token)
            {
                try
                {
                    var channel = token["channel"]?.ToString();
                    var tick = token["tick"];

                    _logger.LogDebug($"Processing order book for channel: {channel}");

                    if (tick != null)
                    {
                        var orderBook = new FameexOrderBook();

                        // Parse asks
                        var asks = tick["asks"] as JArray;
                        if (asks != null)
                        {
                            foreach (var ask in asks)
                            {
                                if (ask is JArray askArray && askArray.Count >= 2)
                                {
                                    orderBook.Asks.Add(new FameexOrderBookEntry
                                    {
                                        Price = askArray[0].Value<decimal>(),
                                        Quantity = askArray[1].Value<decimal>()
                                    });
                                }
                            }
                        }

                        // Parse bids
                        var bids = tick["bids"] as JArray;
                        if (bids != null)
                        {
                            foreach (var bid in bids)
                            {
                                if (bid is JArray bidArray && bidArray.Count >= 2)
                                {
                                    orderBook.Bids.Add(new FameexOrderBookEntry
                                    {
                                        Price = bidArray[0].Value<decimal>(),
                                        Quantity = bidArray[1].Value<decimal>()
                                    });
                                }
                            }
                        }

                        _logger.LogDebug($"Parsed order book: {orderBook.Bids.Count} bids, {orderBook.Asks.Count} asks");
                        _typedHandler.Invoke(message.As(orderBook));
                    }

                    return new CallResult(null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse order book message");
                    return new CallResult(new ServerError("Failed to parse order book message"));
                }
            }

            _logger.LogError($"Invalid message format: expected JToken, got {rawData?.GetType().Name}");
            return new CallResult(new ServerError("Invalid message format"));
        }

        public override Type? GetMessageType(IMessageAccessor message) => typeof(JToken);
    }
}