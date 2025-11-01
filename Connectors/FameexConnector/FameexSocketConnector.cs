using BaseStockConnector;
using BaseStockConnector.Models;
using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Events;
using BaseStockConnector.Models.Instruments;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface;
using BaseStockConnectorInterface.Logger;
using BaseStockConnectorInterface.Models.Instruments;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Clients;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace FameexConnector
{
    public class FameexSocketConnector : IStockSocket<CallResult<UpdateSubscription>>, IStockSocketSubscribe<CallResult<UpdateSubscription>>
    {
        private const string StockName = "FameEX";
        private ILogger _logger;
        private readonly IStock<CallResult<UpdateSubscription>> _connector;
        private readonly ConcurrentDictionary<string, string> _orderInternalIdsDictionary;
        private readonly ConcurrentDictionary<string, UpdateSubscription> _subscriptions = new();
        public FameexSocketClient ClientSocket;
        private List<string> tickerSubscribedInstruments = new();
        public string ApiKey { get; private set; }

        public event EventHandler Connect;
        public event EventHandler<SocketDisconectEventArgs> Disconnect;

        public FameexSocketConnector(IStock<CallResult<UpdateSubscription>> connector, ConcurrentDictionary<string, string> orderInternalIdsDictionary)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _orderInternalIdsDictionary = orderInternalIdsDictionary ?? throw new ArgumentNullException(nameof(orderInternalIdsDictionary));
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy? proxy)
        {
            if (ClientSocket != null) return;

            var credential = (FameexCredential)stockCredential;
            ApiKey = credential.ApiKey;
            _logger = clientLogger;

            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            ClientSocket = new FameexSocketClient(options =>
            {
                options.OutputOriginalData = true;
                options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret);
                options.RateLimiterEnabled = true;
                if (proxy != null)
                {
                    options.Proxy = new CryptoExchange.Net.Objects.ApiProxy($"{proxy.Address.Scheme}://{proxy.Address.Host}", proxy.Address.Port);
                }
            }, logFactory);  
 
 
            await _connector.HttpClient.ConnectAsync(credential, _logger, proxy);

            await Task.CompletedTask;
        }

        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> IndexTickerSubscribeAsync(string instrumentName, InstrumentType instrumentType, Action<TickerUpdateModel> onMessage)
        {
            if (ClientSocket == null) throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync first!");
            if (instrumentType != InstrumentType.Spot) return null;

            tickerSubscribedInstruments.Add(instrumentName);
            var symbols = new List<string> { instrumentName };
            _logger.LogDebug($"Subscribing to ticker for {instrumentName}");

            int retries = 3;
            int delayMs = 5000;
            CallResult<UpdateSubscription> subResult = null;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    subResult = await ClientSocket.SpotApi.SubscribeToTickerAsync(
                        symbols,
                        (data) =>
                        {
                            try
                            {
                                if (data?.Data == null)
                                {
                                    _logger.LogWarning($"No ticker data received for {instrumentName}");
                                    return;
                                }

                                _logger.LogDebug($"Ticker update for {instrumentName}: {JsonConvert.SerializeObject(data.Data)}");
                                onMessage?.Invoke(new TickerUpdateModel
                                {
                                    StockName = StockName,
                                    InstrumentName = instrumentName,
                                    LastPrice = data.Data.Close.GetValueOrDefault(),
                                    TotalVolume = data.Data.Volume.GetValueOrDefault()
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"{StockName} IndexTickerSubscribeAsync error for {instrumentName}");
                            }
                        }, CancellationToken.None);

                    if (subResult.Success)
                        break;

                    _logger.LogWarning($"Subscription attempt {i + 1} failed for {instrumentName}: {subResult.Error?.ToString()}");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Subscription attempt {i + 1} failed for {instrumentName}");
                    await Task.Delay(delayMs);
                }
            }

            if (subResult == null || !subResult.Success)
                _logger.LogError($"Subscription failed for {instrumentName}: {subResult?.Error?.ToString() ?? "Unknown error"}");

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult?.Success ?? false,
                Error = subResult?.Error?.ToString(),
                OriginalObject = subResult
            };
        }

        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> PriceUpdateSubscribeAsync(string instrumentName, InstrumentType instrumentType, Action<PriceUpdateModel> onMessage)
        {
            if (ClientSocket == null) throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync first!");
            if (instrumentType != InstrumentType.Spot) return null;

            tickerSubscribedInstruments.Add(instrumentName);
            var symbols = new List<string> { instrumentName };
            _logger.LogDebug($"Subscribing to trades for {instrumentName}");

            int retries = 3;
            int delayMs = 5000;
            CallResult<UpdateSubscription> subResult = null;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    subResult = await ClientSocket.SpotApi.SubscribeToTradesAsync(
                        symbols,
                        (data) =>
                        {
                            try
                            {
                                if (data?.Data == null || !data.Data.Any())
                                {
                                    _logger.LogWarning($"No trade data received for {instrumentName}");
                                    return;
                                }

                                var latestTrade = data.Data.OrderByDescending(t => t.Timestamp).First();

                                onMessage?.Invoke(new PriceUpdateModel
                                {
                                    StockName = StockName,
                                    InstrumentName = instrumentName,
                                    LastPrice = latestTrade.Price
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"{StockName} PriceUpdateSubscribeAsync error for {instrumentName}");
                            }
                        }, CancellationToken.None);

                    if (subResult.Success)
                        break;

                    _logger.LogWarning($"Subscription attempt {i + 1} failed for {instrumentName}: {subResult.Error?.ToString()}");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Subscription attempt {i + 1} failed for {instrumentName}");
                    await Task.Delay(delayMs);
                }
            }

            if (subResult == null || !subResult.Success)
                _logger.LogError($"Subscription failed for {instrumentName}: {subResult?.Error?.ToString() ?? "Unknown error"}");

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult?.Success ?? false,
                Error = subResult?.Error?.ToString(),
                OriginalObject = subResult
            };
        }

        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> PartialOrderBookSubscribeAsync(string instrumentName, InstrumentType instrumentType, int? level, Action<OrderBook> onMessage)
        {
            if (ClientSocket == null) throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync first!");
            if (instrumentType != InstrumentType.Spot) return null;

            _logger.LogDebug($"Subscribing to order book for {instrumentName} with level {level}");

            int retries = 3;
            int delayMs = 5000;
            CallResult<UpdateSubscription> subResult = null;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    subResult = await ClientSocket.SpotApi.SubscribeToOrderBookAsync(
                        new List<string> { instrumentName },
                        level ?? 20,
                        (data) =>
                        {
                            try
                            {
                                if (data?.Data == null)
                                {
                                    _logger.LogWarning($"No order book data received for {instrumentName}");
                                    return;
                                }

                                onMessage?.Invoke(new OrderBook
                                {
                                    StockName = StockName,
                                    InstrumentName = instrumentName,
                                    OriginalInstrumentName = instrumentName,
                                    InstrumentType = InstrumentType.Spot,
                                    Bids = data.Data.Bids?.Select(b => new OrderBookEntry
                                    {
                                        Price = b.Price,
                                        Amount = b.Quantity
                                    }).ToList() ?? new List<OrderBookEntry>(),
                                    Asks = data.Data.Asks?.Select(a => new OrderBookEntry
                                    {
                                        Price = a.Price,
                                        Amount = a.Quantity
                                    }).ToList() ?? new List<OrderBookEntry>(),
                                    SystemTime = DateTime.UtcNow
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"{StockName} PartialOrderBookSubscribeAsync error for {instrumentName}");
                            }
                        }, CancellationToken.None);

                    if (subResult.Success)
                        break;

                    _logger.LogWarning($"Subscription attempt {i + 1} failed for {instrumentName}: {subResult.Error?.ToString()}");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Subscription attempt {i + 1} failed for {instrumentName}");
                    await Task.Delay(delayMs);
                }
            }

            if (subResult == null || !subResult.Success)
                _logger.LogError($"Subscription failed for {instrumentName}: {subResult?.Error?.ToString() ?? "Unknown error"}");

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult?.Success ?? false,
                Error = subResult?.Error?.ToString(),
                OriginalObject = subResult
            };
        }
        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> OrderUpdateSubscribeAsync(
    InstrumentType instrumentType,
    List<string> instrumentNames,
    Action<OrderUpdate> onMessage)
        {
            if (ClientSocket == null)
            {
                _logger.LogDebug($"${StockName} client is null, call ConnectAsync first!");
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync first!");
            }
                
            if (instrumentType != InstrumentType.Spot)
                return null;
            if(ClientSocket.ClientOptions.ApiCredentials == null)
            {
                _logger.LogDebug($"Credensitals is null!");
                throw new NullReferenceException($"Credensitals is null!");
            }
            _logger.LogDebug($"Subscribing to order updates for {string.Join(",", instrumentNames)}");

            int retries = 3;
            int delayMs = 5000;
            CallResult<UpdateSubscription> subResult = null;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    subResult = await ClientSocket.SpotApi.SubscribeToOrderUpdatesAsync(
                        (data) =>
                        {
                            try
                            {
                                if (data?.Data == null)
                                {
                                    _logger.LogWarning($"No order update data received for {string.Join(",", instrumentNames)}");
                                    return;
                                }

                                _logger.LogDebug($"Order update for {data.Data.Symbol}: {JsonConvert.SerializeObject(data.Data)}");
                                onMessage?.Invoke(new OrderUpdate
                                {
                                    StockName = StockName,
                                    InstrumentName = data.Data.Symbol,
                                    StockOrderId = data.Data.ClientOrderId,
                                    SystemOrderId = data.Data.OrderId,
                                    Price = data.Data.Price,
                                    OrderStatus = data.Data.State switch
                                    {
                                        1 => OrderStatus.New,
                                        2 => OrderStatus.New, // Waiting маппится на New, так как нет Pending в FameEX
                                        3 => OrderStatus.PartiallyFilled,
                                        4 => OrderStatus.Filled,
                                        5 => OrderStatus.Canceled, // PartiallyCancelled маппится на Canceled
                                        6 => OrderStatus.Canceled,
                                        _ => OrderStatus.Rejected // Неизвестные состояния маппятся на Rejected
                                    },
                                    OrderDirection = data.Data.Side == 1 ? Direction.BUY : Direction.SELL,
                                    OrderType = data.Data.OrderType == 1 ? OrderType.Limit : OrderType.Market,
                                    FilledVolume = data.Data.FilledAmount,
                                    Timestamp = data.Data.UpdateTime,
                                    EventTimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Data.UpdateTime).UtcDateTime,
                                    MarkPrice = 0m, // FameEX не предоставляет MarkPrice
                                    IndexPrice = 0m, // FameEX не предоставляет IndexPrice
                                    Fee = 0m, // FameEX не предоставляет Fee
                                    FeeAsset = null // FameEX не предоставляет FeeAsset
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"{StockName} OrderUpdateSubscribeAsync error for {string.Join(",", instrumentNames)}");
                            }
                        }, CancellationToken.None);

                    if (subResult.Success)
                        break;

                    _logger.LogWarning($"Subscription attempt {i + 1} failed for {string.Join(",", instrumentNames)}: {subResult.Error?.ToString()}");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Subscription attempt {i + 1} failed for {string.Join(",", instrumentNames)}");
                    await Task.Delay(delayMs);
                }
            }

            if (subResult == null || !subResult.Success)
                _logger.LogError($"Subscription failed for {string.Join(",", instrumentNames)}: {subResult?.Error?.ToString() ?? "Unknown error"}");

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult?.Success ?? false,
                Error = subResult?.Error?.ToString(),
                OriginalObject = subResult
            };
        }
        public async Task DisconnectAsync()
        {
            if (ClientSocket != null)
            {
                foreach (var sub in _subscriptions)
                {
                    _logger.LogDebug($"Unsubscribed: {sub.Key}");
                }
                _subscriptions.Clear();
                ClientSocket = null;
                Disconnect?.Invoke(this, new SocketDisconectEventArgs());
            }
            _logger.LogInformation("FameEX WebSocket disconnected");
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}