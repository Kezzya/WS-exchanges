using BaseStockConnector.Models.Enums;
using BaseStockConnectorInterface.Models.Enums;
using BaseStockConnectorInterface.Models.Kline;
using ChainApexConnector;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConnectorsTester
{
    public static class ChainApexConnectorTest
    {
        public static async Task TestChainApexConnector()
        {
            Console.WriteLine("=== ChainApex Connector Testing ===");
            
            var config = ChainApexConfig.Load();
            var settings = TestSettings.Load();
            
            var connector = new ChainApexConnector.ChainApexConnector(config.BaseUrl, config.WebSocketUrl);
            var logger = GetLogger(connector.StockName);
            
            var credential = new ChainApexCredential
            {
                ApiKey = config.ApiKey,
                Secret = config.SecretKey
            };
            
            try
            {
                Console.WriteLine("\n1. Connecting to ChainApex...");
                await connector.HttpClient.ConnectAsync(credential, logger, (System.Net.WebProxy)null!);
                Console.WriteLine("[OK] Connected successfully");
                
                Console.WriteLine("\n2. Testing Public API Methods:");
                
                // Server Time
                Console.WriteLine("\n   2.1 Get Server Time...");
                var serverTime = await connector.HttpClient.GetServerTimestamp();
                Console.WriteLine($"   [OK] Server Time: {serverTime:yyyy-MM-dd HH:mm:ss}");
                
                // Get All Symbols
                Console.WriteLine("\n   2.2 Get All Symbols...");
                var symbols = await connector.HttpClient.GetCoinInfoAsync(InstrumentType.Spot);
                Console.WriteLine($"   [OK] Found {symbols.Count} symbols");
                foreach (var symbol in symbols.Take(5))
                {
                    Console.WriteLine($"      - {symbol.InstrumentName} ({symbol.BaseCurrency}/{symbol.QuoteCurrency})");
                    Console.WriteLine($"        Min Volume: {symbol.MinVolumeInBaseCurrency}, Price Step: {symbol.MinMovement}");
                }
                
                // Get Symbol Info
                var testSymbol = settings.DefaultSymbol.ToUpper();
                Console.WriteLine($"\n   2.3 Get Symbol Info for {testSymbol}...");
                var symbolInfo = await connector.HttpClient.GetCoinInfoAsync(testSymbol, InstrumentType.Spot);
                Console.WriteLine($"   [OK] Symbol: {symbolInfo.InstrumentName}");
                Console.WriteLine($"      Base: {symbolInfo.BaseCurrency}, Quote: {symbolInfo.QuoteCurrency}");
                
                // Get 24h Volume
                Console.WriteLine($"\n   2.4 Get 24h Volume for {testSymbol}...");
                var volume = await connector.HttpClient.Get24hVolume(testSymbol, InstrumentType.Spot);
                Console.WriteLine($"   [OK] 24h Volume: {volume:N2}");
                
                // Get Order Book
                Console.WriteLine($"\n   2.5 Get Order Book for {testSymbol}...");
                var orderBook = await connector.HttpClient.GetDepthAsync(InstrumentType.Spot, testSymbol, settings.OrderBookDepth);
                Console.WriteLine($"   [OK] Order Book:");
                Console.WriteLine($"      Bids: {orderBook.Bids.Count} levels");
                Console.WriteLine($"      Asks: {orderBook.Asks.Count} levels");
                if (orderBook.Bids.Count > 0)
                    Console.WriteLine($"      Best Bid: {orderBook.Bids[0].Price:F2} @ {orderBook.Bids[0].Amount:F8}");
                if (orderBook.Asks.Count > 0)
                    Console.WriteLine($"      Best Ask: {orderBook.Asks[0].Price:F2} @ {orderBook.Asks[0].Amount:F8}");
                
                // Get Klines
                Console.WriteLine($"\n   2.6 Get Klines for {testSymbol}...");
                var klines = await connector.HttpClient.GetKlines(
                    testSymbol, 
                    KlineInterval.OneHour, 
                    DateTime.UtcNow.AddHours(-24), 
                    DateTime.UtcNow, 
                    10, 
                    InstrumentType.Spot);
                Console.WriteLine($"   [OK] Klines: {klines.Count} candles");
                foreach (var kline in klines.Take(3))
                {
                    Console.WriteLine($"      {kline.OpenTime:HH:mm} - O:{kline.OpenPrice:F2} H:{kline.HighPrice:F2} L:{kline.LowPrice:F2} C:{kline.ClosePrice:F2} V:{kline.Volume:F2}");
                }
                
                // Get Last Trade
                Console.WriteLine($"\n   2.7 Get Last Trade for {testSymbol}...");
                var lastTrade = await connector.HttpClient.GetLastTrade(testSymbol, InstrumentType.Spot);
                Console.WriteLine($"   [OK] Last Trade:");
                Console.WriteLine($"      Price: {lastTrade.Price:F2}");
                Console.WriteLine($"      Volume: {lastTrade.Volume:F8}");
                Console.WriteLine($"      Direction: {lastTrade.Direction}");
                Console.WriteLine($"      Time: {lastTrade.TransactionDate:yyyy-MM-dd HH:mm:ss}");
                
                Console.WriteLine("\n3. Testing Private API Methods:");
                
                // Get All Balances
                Console.WriteLine("\n   3.1 Get All Balances...");
                var balances = await connector.HttpClient.GetAllBalances(BalanceType.Spot);
                Console.WriteLine($"   [OK] Found {balances.Count} balances");
                var nonZeroBalances = balances.Where(b => b.AvailableBalance > 0 || b.FrozenBalance > 0).Take(5);
                foreach (var balance in nonZeroBalances)
                {
                    Console.WriteLine($"      {balance.Currency}: Available={balance.AvailableBalance:F8}, Frozen={balance.FrozenBalance:F8}");
                }
                
                // Get Specific Balance
                Console.WriteLine("\n   3.2 Get USDT Balance...");
                var usdtBalance = await connector.HttpClient.GetBalance("USDT", BalanceType.Spot);
                Console.WriteLine($"   [OK] USDT Balance:");
                if (usdtBalance.Balances.Any())
                {
                    var bal = usdtBalance.Balances.First();
                    Console.WriteLine($"      Available: {bal.AvalibleBalance:F8}");
                    Console.WriteLine($"      Frozen: {bal.FreezedBalance:F8}");
                }
                
                // Get Active Orders
                Console.WriteLine($"\n   3.3 Get Active Orders for {testSymbol}...");
                var activeOrders = await connector.HttpClient.GetActiveOrdersAsync(testSymbol, InstrumentType.Spot);
                Console.WriteLine($"   [OK] Active Orders: {activeOrders.Count}");
                foreach (var order in activeOrders.Take(3))
                {
                    Console.WriteLine($"      Order {order.StockOrderId}: {order.Direction} {order.Volume:F8} @ {order.Price:F2} - Success: {order.Success}");
                }
                
                // Get Order History
                Console.WriteLine($"\n   3.4 Get Order History for {testSymbol}...");
                var orderHistory = await connector.HttpClient.GetOrderHistoryForPeriodWithFee(
                    testSymbol,
                    DateTime.UtcNow.AddDays(-7),
                    DateTime.UtcNow,
                    InstrumentType.Spot);
                Console.WriteLine($"   [OK] Order History: {orderHistory.Count} orders");
                foreach (var order in orderHistory.Take(3))
                {
                    Console.WriteLine($"      {order.DateCreated:yyyy-MM-dd HH:mm} - {order.Direction} {order.Volume:F8} @ {order.Price:F2}");
                    Console.WriteLine($"         Fee: {order.Fee:F8} {order.FeeCurrency}");
                }
                
                // Test Rate Limit Snapshot
                Console.WriteLine("\n   3.5 Get Rate Limit Snapshot...");
                var rateLimitSnapshot = await connector.HttpClient.GetRateLimitSnapshot();
                Console.WriteLine($"   [OK] Rate Limit Snapshot:");
                Console.WriteLine($"      Exchange: {rateLimitSnapshot.ExchangeName}");
                Console.WriteLine($"      Timestamp: {rateLimitSnapshot.Timestamp:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"      Rate Limits: {rateLimitSnapshot.RateLimits.Count}");
                foreach (var limit in rateLimitSnapshot.RateLimits)
                {
                    Console.WriteLine($"      - {limit.TrackerKey}: {limit.Current}/{limit.Limit} (Period: {limit.Period})");
                }
                
                Console.WriteLine("\n4. Testing WebSocket API:");
                
                // Connect WebSocket
                Console.WriteLine("\n   4.1 Connecting to WebSocket...");
                await connector.SocketClient.ConnectAsync(credential, logger, (System.Net.WebProxy)null!);
                Console.WriteLine("   [OK] WebSocket connected");
                
                // Test Order Book Subscription
                Console.WriteLine($"\n   4.2 Subscribing to Order Book for {testSymbol}...");
                var orderBookUpdateCount = 0;
                var orderBookSub = await connector.SocketClient.PartialOrderBookSubscribeAsync(
                    testSymbol,
                    InstrumentType.Spot,
                    10,
                    orderBook =>
                    {
                        orderBookUpdateCount++;
                        if (orderBookUpdateCount <= 3)
                        {
                            Console.WriteLine($"      [Order Book Update #{orderBookUpdateCount}]");
                            if (orderBook.Bids.Count > 0 && orderBook.Asks.Count > 0)
                            {
                                Console.WriteLine($"         Best Bid: {orderBook.Bids[0].Price:F2} @ {orderBook.Bids[0].Amount:F8}");
                                Console.WriteLine($"         Best Ask: {orderBook.Asks[0].Price:F2} @ {orderBook.Asks[0].Amount:F8}");
                            }
                        }
                    });
                
                if (orderBookSub.IsSuccess)
                {
                    Console.WriteLine("   [OK] Order Book subscription successful");
                }
                else
                {
                    Console.WriteLine($"   [ERROR] Order Book subscription failed: {orderBookSub.Error}");
                }
                
                // Test Ticker Subscription
                Console.WriteLine($"\n   4.3 Subscribing to Ticker for {testSymbol}...");
                var tickerUpdateCount = 0;
                var tickerSub = await connector.SocketClient.IndexTickerSubscribeAsync(
                    testSymbol,
                    InstrumentType.Spot,
                    ticker =>
                    {
                        tickerUpdateCount++;
                        if (tickerUpdateCount <= 3)
                        {
                            Console.WriteLine($"      [Ticker Update #{tickerUpdateCount}] Last Price: {ticker.LastPrice:F2}, Total Volume: {ticker.TotalVolume:F2}");
                        }
                    });
                
                if (tickerSub.IsSuccess)
                {
                    Console.WriteLine("   [OK] Ticker subscription successful");
                }
                else
                {
                    Console.WriteLine($"   [ERROR] Ticker subscription failed: {tickerSub.Error}");
                }
                
                // Test Trade Updates Subscription
                Console.WriteLine($"\n   4.4 Subscribing to Trades for {testSymbol}...");
                var tradeUpdateCount = 0;
                var tradeSub = await connector.SocketClient.PriceUpdateSubscribeAsync(
                    testSymbol,
                    InstrumentType.Spot,
                    trade =>
                    {
                        tradeUpdateCount++;
                        if (tradeUpdateCount <= 3)
                        {
                            Console.WriteLine($"      [Trade Update #{tradeUpdateCount}] Last Price: {trade.LastPrice:F2}");
                        }
                    });
                
                if (tradeSub.IsSuccess)
                {
                    Console.WriteLine("   [OK] Trade subscription successful");
                }
                else
                {
                    Console.WriteLine($"   [ERROR] Trade subscription failed: {tradeSub.Error}");
                }
                
                // Listen for updates
                var listenSeconds = 10;
                Console.WriteLine($"\n   4.5 Listening to WebSocket updates for {listenSeconds} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(listenSeconds));
                
                Console.WriteLine($"\n   [INFO] Received updates: OrderBook={orderBookUpdateCount}, Ticker={tickerUpdateCount}, Trades={tradeUpdateCount}");
                
                // Disconnect WebSocket
                await connector.SocketClient.DisconnectAsync();
                Console.WriteLine("   [OK] WebSocket disconnected");
                
                Console.WriteLine("\n[SUCCESS] All HTTP and WebSocket tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] Error during testing: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                await connector.HttpClient.DisconnectAsync();
                Console.WriteLine("\n[OK] Disconnected from ChainApex");
            }
        }
        
        private static Microsoft.Extensions.Logging.ILogger GetLogger(string name)
        {
            var workingDirectory = Directory.GetCurrentDirectory();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()                
                .WriteTo.Console()
                .CreateLogger();
            var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);
            var logger = loggerFactory.CreateLogger(name);
            return logger;
        }
    }
}

