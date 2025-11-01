using BaseStockConnector.Models;
using BaseStockConnector.Models.Enums;
using BaseStockConnector.Models.Events;
using BaseStockConnector.Models.Instruments;
using BaseStockConnector.Models.Orders;
using BaseStockConnectorInterface;
using BaseStockConnectorInterface.Logger;
using BaseStockConnectorInterface.Models.Instruments;
using Microsoft.Extensions.Logging;
using ChainApex.Net.Clients;
using StockConnector;
using System.Net;
using BaseStockConnectorInterface.Helper;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System.Collections.Concurrent;

namespace ChainApexConnector
{
    public class ChainApexSocketConnector : IStockSocket<CallResult<UpdateSubscription>>
    {
        private const string StockName = "ChainApex";

        public ChainApexSocketClient ClientSocket;
        private readonly IStock<CallResult<UpdateSubscription>> _connector;
        private ILogger _logger;

        public event EventHandler Connect;
        public event EventHandler<SocketDisconectEventArgs> Disconnect;

        private ConcurrentDictionary<string, string> _orderInternalIdsDictionary;

        public string ApiKey { get; private set; }

        public ChainApexSocketConnector(IStock<CallResult<UpdateSubscription>> connector, ConcurrentDictionary<string, string> orderInternalIdsDictionary)
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

            var credential = new ChainApexCredential();
            _logger = clientLogger;
            try
            {
                credential = (ChainApexCredential)stockCredential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"{StockName} ConnectAsync error");
            }

            ApiKey = credential.ApiKey;
            var logFactory = new LoggerFactory();
            logFactory.AddProvider(new ExchangeLoggerProvider(new PrefixedLogger(_logger, StockName)));

            _logger.LogInformation($"[DEBUG WebSocket] Creating ChainApexSocketClient");
            _logger.LogInformation($"[DEBUG WebSocket] WebSocket URL will be from ChainApexEnvironment.Live");

            ClientSocket = new ChainApexSocketClient(options =>
            {
                options.OutputOriginalData = true;
                options.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials(credential.ApiKey, credential.Secret);
                options.RateLimiterEnabled = true;
                if (proxy != null)
                {
                    options.Proxy = new CryptoExchange.Net.Objects.ApiProxy($"{proxy.Address.Scheme}://{proxy.Address.Host}", proxy.Address.Port);
                }
                
                _logger.LogInformation($"[DEBUG WebSocket] Final WebSocket URL: {options.Environment.PublicSocketClientAddress}");
            }, logFactory);

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

            try
            {
                var result = await ClientSocket.SpotApi.SubscribeToTickerUpdatesAsync(
                    instrumentName,
                    data =>
                    {
                        var ticker = data.Data;
                        onMessage(new TickerUpdateModel
                        {
                            InstrumentName = instrumentName,
                            StockName = StockName,
                            LastPrice = ticker.Close,
                            TotalVolume = ticker.Volume
                        });
                    });

                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = result.Success,
                    Error = result.Error?.Message,
                    OriginalObject = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{StockName} IndexTickerSubscribeAsync error for {instrumentName}");
                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    OriginalObject = null
                };
            }
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

            try
            {
                var result = await ClientSocket.SpotApi.SubscribeToTradeUpdatesAsync(
                    instrumentName,
                    data =>
                    {
                        foreach (var trade in data.Data)
                        {
                            onMessage(new PriceUpdateModel
                            {
                                InstrumentName = instrumentName,
                                StockName = StockName,
                                LastPrice = trade.Price
                            });
                        }
                    });

                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = result.Success,
                    Error = result.Error?.Message,
                    OriginalObject = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{StockName} PriceUpdateSubscribeAsync error for {instrumentName}");
                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    OriginalObject = null
                };
            }
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

            try
            {
                var result = await ClientSocket.SpotApi.SubscribeToDepthUpdatesAsync(
                    instrumentName,
                    data =>
                    {
                        var depth = data.Data;
                        var orderBook = new OrderBook
                        {
                            InstrumentName = instrumentName,
                            StockName = StockName,
                            Bids = depth.Buys.Select(b => new OrderBookEntry
                            {
                                Price = b[0],
                                Amount = b[1]
                            }).ToList(),
                            Asks = depth.Asks.Select(a => new OrderBookEntry
                            {
                                Price = a[0],
                                Amount = a[1]
                            }).ToList(),
                            StockEventTime = DateTime.UtcNow,
                            SystemTime = DateTime.UtcNow
                        };

                        onMessage(orderBook);
                    });

                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = result.Success,
                    Error = result.Error?.Message,
                    OriginalObject = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{StockName} PartialOrderBookSubscribeAsync error for {instrumentName}");
                return new SubscriptionResult<CallResult<UpdateSubscription>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    OriginalObject = null
                };
            }
        }

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

            // ChainApex WebSocket API doesn't support order update subscriptions based on the provided documentation
            // Order updates should be polled via HTTP API or handled through a different mechanism
            await Task.CompletedTask;
            
            return new SubscriptionResult<CallResult<UpdateSubscription>>
            {
                IsSuccess = false,
                Error = "ChainApex WebSocket API does not support order update subscriptions. Use HTTP API polling instead.",
                OriginalObject = null
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
    }
}
