using BaseStockConnector.Models;
using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Events;
using BaseStockConnector.Models.Instruments;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface;
using BaseStockConnectorInterface.Logger;
using BaseStockConnectorInterface.Models.Instruments;
using Microsoft.Extensions.Logging;
using Digifinex.Net.Clients;
using StockConnector;
using System.Net;
using BaseStockConnectorInterface.Helper;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Digifinex.Net.Objects.Models.Socket;
using System.Collections.Concurrent;
using Digifinex.Net.Objects.Models.ExchangeData;
using DigifinexOrderType = Digifinex.Net.Objects.Models.Socket.DigifinexOrderType;

namespace DigifinexConnector
{
    public class DigifinexSocketConnector : IStockSocket<CallResult<UpdateSubscription>>
    {
        private const string StockName = "Digifinex";

        public DigifinexSocketClient ClientSocket;
        private readonly IStock<CallResult<UpdateSubscription>> _connector;
        private ILogger _logger;

        public event EventHandler Connect;
        public event EventHandler<SocketDisconectEventArgs> Disconnect;

        private ConcurrentDictionary<string, string> _orderInternalIdsDictionary;

        public string ApiKey { get; private set; }

        private List<string> tickerSubscribedInstruments = [];

        public DigifinexSocketConnector(IStock<CallResult<UpdateSubscription>> connector,  ConcurrentDictionary<string, string> orderInternalIdsDictionary)
        {
            _orderInternalIdsDictionary = orderInternalIdsDictionary;
            _connector = connector;
        }

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy? proxy)
        {
            if (ClientSocket != null)
            {
                return;
            }

            var credential = new DigifinexCredential();
            _logger = clientLogger;
            try
            {
                credential = (DigifinexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"{StockName} ConnectAsync error");
            }

            ApiKey = credential.ApiKey;
            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            ClientSocket = new DigifinexSocketClient(options =>
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

        #region Ready

        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> IndexTickerSubscribeAsync(string instrumentName, InstrumentType instrumentType, Action<TickerUpdateModel> onMessage)
        {
            if (ClientSocket == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return null;
            }

            tickerSubscribedInstruments.Add(instrumentName);
            var subResult = await ClientSocket.SpotApi.SubscribeToTickerAsync(
                [instrumentName],
                (data) =>
                {
                    try
                    {
                        if (data is not { Data: not null })
                        {
                            return;
                        }

                        var ticker = data.Data.FirstOrDefault(x => x.Symbol.Equals(instrumentName, StringComparison.OrdinalIgnoreCase));
                        if (ticker != null)
                        {
                            onMessage?.Invoke(new TickerUpdateModel()
                            {
                                StockName = StockName,
                                InstrumentName = instrumentName,
                                LastPrice = ticker.Last.GetValueOrDefault(),
                                TotalVolume = ticker.QuoteVolume24h.GetValueOrDefault()
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{StockName} IndexTickerSubscribeAsync error");
                    }
                });

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult.Success,
                Error = subResult.Error?.ToString(),
                OriginalObject = subResult
            };
        }

        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> PriceUpdateSubscribeAsync(string instrumentName, InstrumentType instrumentType, Action<PriceUpdateModel> onMessage)
        {
            if (ClientSocket == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return null;
            }

            tickerSubscribedInstruments.Add(instrumentName);
            var subResult = await ClientSocket.SpotApi.SubscribeToTradesAsync(
                [instrumentName],
                (data) =>
                {
                    try
                    {
                        if (data is { Data: not null })
                        {
                            var trades = data.Data.Trades;
                            if (trades.Any())
                            {
                                onMessage?.Invoke(new PriceUpdateModel
                                {
                                    StockName = StockName,
                                    InstrumentName = instrumentName,
                                    LastPrice = trades.First().Price
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{StockName} PriceUpdateSubscribeAsync error");
                    }
                });

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult.Success,
                Error = subResult.Error?.ToString(),
                OriginalObject = subResult
            };
        }

        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> PartialOrderBookSubscribeAsync(string instrumentName, InstrumentType instrumentType, int? level, Action<OrderBook> onMessage)
        {
            if (ClientSocket == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return null;
            }

            DigifinexDepth orderBook = null;
    
            var subResult = await ClientSocket.SpotApi.SubscribeToDepthAsync(
                [instrumentName],
                (data) =>
                {
                    try
                    {
                        if (data?.Data == null)
                        {
                            return;
                        }

                        if (data.Data.Clean) // Snapshot
                        {
                            orderBook = data.Data.Depth;
                        }
                        else // Update
                        {
                            if (orderBook == null)
                                return;

                            var asks = data.Data.Depth.Asks.OrderBy(i => i.Price);
                            var bids = data.Data.Depth.Bids.OrderByDescending(i => i.Price);

                            foreach (var ask in asks)
                            {
                                var entry = orderBook.Asks.FirstOrDefault(x => x.Price == ask.Price);
                                if (entry != null)
                                {
                                    if (ask.Quantity == 0)
                                        orderBook.Asks.Remove(entry);
                                    else
                                        entry.Quantity = ask.Quantity;
                                }
                                else
                                {
                                    orderBook.Asks.Add(ask);
                                }
                            }

                            foreach (var bid in bids)
                            {
                                var entry = orderBook.Bids.FirstOrDefault(x => x.Price == bid.Price);
                                if (entry != null)
                                {
                                    if (bid.Quantity == 0)
                                        orderBook.Bids.Remove(entry);
                                    else
                                        entry.Quantity = bid.Quantity;
                                }
                                else
                                {
                                    orderBook.Bids.Add(bid);
                                }
                            }
                        }

                        if (orderBook != null)
                        {
                            var asks = orderBook.Asks.OrderBy(i => i.Price);
                            var bids = orderBook.Bids.OrderByDescending(i => i.Price);

                            onMessage?.Invoke(new OrderBook
                            {
                                StockName = StockName,
                                InstrumentName = instrumentName,
                                OriginalInstrumentName = instrumentName,
                                InstrumentType = InstrumentType.Spot,
                                Asks = asks.Take(level ?? 20).Select(x => new OrderBookEntry
                                {
                                    Price = x.Price,
                                    Amount = x.Quantity,
                                }).ToList(),
                                Bids = bids.Take(level ?? 20).Select(x => new OrderBookEntry
                                {
                                    Price = x.Price,
                                    Amount = x.Quantity,
                                }).ToList(),
                                SystemTime = DateTime.UtcNow
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{StockName} PartialOrderBookSubscribeAsync error");
                    }
                });

            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = subResult.Success,
                Error = subResult.Error?.ToString(),
                OriginalObject = subResult
            };
        }

        public async Task DisconnectAsync()
        {
            if (ClientSocket != null)
                await ClientSocket.UnsubscribeAllAsync();
        }

        public void Dispose()
        {
            if (ClientSocket != null)
                ClientSocket.Dispose();
        }

        #endregion

        //IMPORTANT: The status must match the filled volume. The filled volume and the total volume are in the base currency!
        public async Task<SubscriptionResult<CallResult<UpdateSubscription>>> OrderUpdateSubscribeAsync(InstrumentType instrumentType, List<string> instrumentNames, Action<OrderUpdate> onMessage)
        {
            if (ClientSocket == null)
            {
                throw new InvalidOperationException($"{StockName} client is null, call ConnectAsync method first!");
            }

            if (instrumentType != InstrumentType.Spot)
            {
                return null;
            }

            try
            {
                var subResult = await ClientSocket.SpotApi.SubscribeToOrdersAsync(instrumentNames, data =>
                {
                    if (data?.Data == null)
                    {
                        return;
                    }

                    foreach (var order in data.Data)
                    {
                        if (instrumentNames.FirstOrDefault(x => x.Equals(order.Symbol, StringComparison.InvariantCultureIgnoreCase)) == null)
                        {
                            continue;
                        }

                        var orderUpdate = new OrderUpdate
                        {
                            StockName = StockName,
                            OriginalInstrumentName = order.Symbol,
                            InstrumentName = order.Symbol,
                            OrderType = order.Type == DigifinexOrderType.Limit ? OrderType.Limit : OrderType.Market,
                            InstrumentType = instrumentType,
                            OrderDirection = order.Side == DigifinexOrderSide.Buy ? Direction.BUY : Direction.SELL,
                            StockOrderId = order.Id,
                            SystemOrderId = GetOrderGuid(order.Id),
                            FilledVolume = order.Filled,
                            Price = order.Price,
                            EventTimeStamp = order.Timestamp,
                            Timestamp = order.Timestamp.ToUnixTimeMilliseconds(),
                            IndexPrice = 0m,
                            MarkPrice = 0m,
                            OrderStatus = GetOrderStatus(order.Status),
                        };

                        onMessage.Invoke(orderUpdate);

                        if (order.Status is DigifinexOrderStatus.FullyFilled or DigifinexOrderStatus.CanceledUnfilled or DigifinexOrderStatus.CanceledPartiallyFilled)
                        {
                            TryRemoveOrderGuid(order.Id);
                        }
                    }
                });

                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = subResult.Success,
                    Error = subResult.Error?.ToString(),
                    OriginalObject = subResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{StockName} OrderUpdateSubscribeAsync error");
                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    OriginalObject = null
                };
            }
        }

        private OrderStatus GetOrderStatus(DigifinexOrderStatus status)
        {
            return status switch
            {
                DigifinexOrderStatus.Unfilled => OrderStatus.New,
                DigifinexOrderStatus.PartiallyFilled => OrderStatus.PartiallyFilled,
                DigifinexOrderStatus.FullyFilled => OrderStatus.Filled,
                DigifinexOrderStatus.CanceledUnfilled => OrderStatus.Canceled,
                DigifinexOrderStatus.CanceledPartiallyFilled => OrderStatus.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        private string GetOrderGuid(string externalOrderId)
        {
            var key = new string(externalOrderId.TakeLast(9).ToArray());

            if (_orderInternalIdsDictionary.TryGetValue(key, out var internalGuid))
            {
                return internalGuid;
            }

            _logger.LogWarning($"GetOrderGuid no guid for:{internalGuid}, key:{key} count:{_orderInternalIdsDictionary.Count} {StockName} ");
            return Guid.Empty.ToString();
        }

        private bool TryRemoveOrderGuid(string externalOrderId)
        {
            var key = new string(externalOrderId.TakeLast(9).ToArray());
            if (_orderInternalIdsDictionary.ContainsKey(key))
            {
                _orderInternalIdsDictionary.TryRemove(key, out _);
                return true;
            }

            _logger.LogWarning($"TryRemoveOrderGuid no guid for:{externalOrderId}, key:{key} count:{_orderInternalIdsDictionary.Count} {StockName} ");
            return false;
        }
    }
}