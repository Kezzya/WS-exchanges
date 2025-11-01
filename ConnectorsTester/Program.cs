
using BaseStockConnector.Models.Enums;
using BaseStockConnectorInterface.Models.Enums;
using BaseStockConnectorInterface.Models.Kline;
using DigifinexConnector;
using FameexConnector;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace ConnectorsTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "chainapex-connector")
            {
                Console.WriteLine("=== ChainApex Connector Testing ===");
                Task.Run(async () => await ChainApexConnectorTest.TestChainApexConnector()).Wait();
            }
            else if (args.Length > 0 && args[0].ToLower() == "chainapex-chainstream")
            {
                Console.WriteLine("=== ChainApex ChainStream Protocol Testing ===");
                Task.Run(async () => await ChainApexChainStreamTest.TestChainStreamProtocol()).Wait();
            }
            else if (args.Length > 0 && args[0].ToLower() == "digifinex")
            {
                Console.WriteLine("=== Digifinex Connector Testing ===");
                Task.Run(async () => await TestDigifinexConnector()).Wait();
            }
            else if (args.Length > 0 && args[0].ToLower() == "fameex")  
            {
                Console.WriteLine("=== Fameex Connector Testing ===");
                Task.Run(async () => await TestFameexConnector()).Wait();
            }
            else
            {
                Console.WriteLine("=== Available Test Options ===");
                Console.WriteLine("Usage: dotnet run <option>");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  chainapex-connector    - Test ChainApex connector (HTTP API + WebSocket)");
                Console.WriteLine("  chainapex-chainstream  - Test ChainStream alternative WebSocket protocol");
                Console.WriteLine("  digifinex             - Test Digifinex connector");
                Console.WriteLine("  fameex             - Test Digifinex connector");
                Console.WriteLine();
                Console.WriteLine("Example: dotnet run chainapex-connector");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task TestDigifinexConnector()
        {
            var instrumentName = "XRP_USDT"; // Digifinex uses underscore format


            var connector = new DigifinexConnector.DigifinexConnector();
            var logger = GetLogger(connector.StockName);

            await connector.HttpClient.ConnectAsync(
                new DigifinexCredential()
                {
                    ApiKey =  "test",
                    Secret =  "test",
                },
                logger);

            await connector.SocketClient.ConnectAsync(
                new DigifinexCredential()
                {
                    ApiKey =  "test",
                    Secret =  "test",
                },
                logger);

            // --- WebSocket Subscriptions ---

            //OK
            //var orderBookSub = await connector.SocketClient.PartialOrderBookSubscribeAsync(
            //    instrumentName,
            //    InstrumentType.Spot,
            //    10,
            //    data =>
            //    {
            //        Console.WriteLine($"OrderBook Update: {JsonConvert.SerializeObject(data)}");
            //    });

            //OK
            //var tickerSub = await connector.SocketClient.IndexTickerSubscribeAsync(
            //    instrumentName,
            //    InstrumentType.Spot,
            //    data =>
            //    {
            //        Console.WriteLine($"Ticker Update: {JsonConvert.SerializeObject(data)}");
            //    });

            //OK
            //var priceSub = await connector.SocketClient.PriceUpdateSubscribeAsync(
            //    instrumentName,
            //    InstrumentType.Spot,
            //    data =>
            //    {
            //        Console.WriteLine($"Price Update: {JsonConvert.SerializeObject(data)}");
            //    });

            ///IMPORTANT: The status must match the filled volume. The filled volume and the total volume are in the base currency!
            var orderUpdateSub = await connector.SocketClient.OrderUpdateSubscribeAsync(
                InstrumentType.Spot,
                new List<string> { instrumentName },
                data =>
                {
                    Console.WriteLine($"Order Update: {data.FilledVolume}");
                });

            //await Task.Delay(TimeSpan.FromMinutes(1));
            //var newOrder123 = await connector.HttpClient.MakeOrderAsync(
            //    InstrumentType.Spot,
            //    OrderType.Limit,
            //    Direction.BUY,
            //    PositionOrderType.Open,
            //    "DOGE_USDT",
            //    2m,
            //    2,
            //    Guid.NewGuid().ToString("N"));

            // --- HTTP API Tests ---
            await Task.Delay(600000000);

            // Server time
            //var serverTime = await connector.HttpClient.GetServerTimestamp();

            //// Coin info
            //var coinInfo = await connector.HttpClient.GetCoinInfoAsync("doge_usdt", InstrumentType.Spot);//OK

            //// All coins
            //var allCoins = await connector.HttpClient.GetCoinInfoAsync(InstrumentType.Spot);//OK

            //// Order book
            //var orderBook = await connector.HttpClient.GetDepthAsync(InstrumentType.Spot, instrumentName, 10);//OK

            //// Klines
            //var klines = await connector.HttpClient.GetKlines(instrumentName, KlineInterval.OneHour,
            //    DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, 100, InstrumentType.Spot);//OK

            //// Last trade
            //var lastTrade = await connector.HttpClient.GetLastTrade(instrumentName, InstrumentType.Spot);//OK

            //// Tickers
            //var tickers = await connector.HttpClient.FetchTickersAsync();//OK

            // Balances
            var balance = await connector.HttpClient.GetBalance("USDT", BalanceType.Spot);//OK

            var allBalances = await connector.HttpClient.GetAllBalances(BalanceType.Spot);//OK

            //// Active orders
            var activeOrders = await connector.HttpClient.GetActiveOrdersAsync(instrumentName, InstrumentType.Spot);//OK

            //// Order history
            //var orderHistory = await connector.HttpClient.GetOrderHistory(
            //    instrumentName, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, InstrumentType.Spot);//OK

            //// Order history with fee
            //var orderHistoryWithFee = await connector.HttpClient.GetOrderHistoryForPeriodWithFee(
            //    instrumentName, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, InstrumentType.Spot);//OK

            ////// Deposit / Withdraw / Transfer history (requires permissions on API key)
            //var deposits = await connector.HttpClient.GetDepositHistory("USDT", DateTime.UtcNow.AddDays(-30), 50);//OK
            //var withdraws = await connector.HttpClient.GetWithdrawHistory("USDT", DateTime.UtcNow.AddDays(-30), 50);//OK
            //var transfers = await connector.HttpClient.GetTransferHistory("USDT", 50, DateTime.UtcNow.AddDays(-30));//OK

            // Place and cancel a test order
            var newOrder = await connector.HttpClient.MakeOrderAsync(
                InstrumentType.Spot,
                OrderType.Limit,
                Direction.BUY,
                PositionOrderType.Open,
                instrumentName,
                2m,
                2,
                Guid.NewGuid().ToString("N"));

            var orderInfo = await connector.HttpClient.GetOrderInfo(instrumentName, newOrder.StockOrderId, InstrumentType.Spot);

            var cancelResult = await connector.HttpClient.CancelOrderAsync(InstrumentType.Spot, instrumentName, newOrder.StockOrderId);

            Console.WriteLine("Digifinex connector test completed successfully!");

            // Keep the subscriptions alive
            await Task.Delay(9000000);
        }
        private static async Task TestFameexConnector()
        {
            var instrumentName = "LTCUSDT";
            var connector = new FameexConnector.FameexConnector();
            var logger = GetLogger(connector.StockName);

            // Подключение HTTP
            await connector.HttpClient.ConnectAsync(
                new FameexCredential
                {
                    ApiKey = "1b8f2c077c770995f442c993d1c38453",
                    Secret = "d7653ca89808b7be3e4490abc192a02a"
                },
                logger);

            // Подключение WebSocket
            await connector.SocketClient.ConnectAsync(
                new FameexCredential
                {
                    ApiKey = "1b8f2c077c770995f442c993d1c38453",
                    Secret = "d7653ca89808b7be3e4490abc192a02a"
                },
                logger);

            Console.WriteLine("✅ Connected to FameEX HTTP and WebSocket");

            // ===================================================================
            // 🔥 ГЛАВНОЕ: ПОДПИСКА НА ОБНОВЛЕНИЯ ОРДЕРОВ ПЕРЕД СОЗДАНИЕМ ОРДЕРА
            // ===================================================================
            Console.WriteLine("\n📡 Subscribing to order updates...");

            var orderUpdateSub = await connector.SocketClient.OrderUpdateSubscribeAsync(
                InstrumentType.Spot,
                new List<string> { instrumentName }, // Можно пустой список для всех символов
                data =>
                {
                    Console.WriteLine($"\n🔔 ORDER UPDATE:");
                    Console.WriteLine($"  Symbol: {data.InstrumentName}");
                    Console.WriteLine($"  OrderId: {data.SystemOrderId}");
                    Console.WriteLine($"  ClientOrderId: {data.StockOrderId}");
                    Console.WriteLine($"  Status: {data.OrderStatus}");
                    Console.WriteLine($"  Side: {data.OrderDirection}");
                    Console.WriteLine($"  Price: {data.Price}");
                    Console.WriteLine($"  FilledVolume: {data.FilledVolume}");
                    Console.WriteLine($"  Timestamp: {data.EventTimeStamp}");
                });

            if (orderUpdateSub.IsSuccess)
            {
                Console.WriteLine("✅ Order updates subscription successful!");
            }
            else
            {
                Console.WriteLine($"❌ Order updates subscription failed: {orderUpdateSub.Error}");
                return; // Выходим, если подписка не удалась
            }

            // Небольшая задержка для установки подключения
            await Task.Delay(2000);

            // ===================================================================
            // БЫСТРЫЕ ПРОВЕРКИ (можно закомментировать после первого теста)
            // ===================================================================

            // Время сервера
            var serverTime = await connector.HttpClient.GetServerTimestamp();
            Console.WriteLine($"\n⏰ Server Time: {serverTime}");

            // Информация о монете
            var coinInfo = await connector.HttpClient.GetCoinInfoAsync(instrumentName, InstrumentType.Spot);
            Console.WriteLine($"\n💰 Coin Info for {instrumentName}:");
            Console.WriteLine($"  MinVolume: {coinInfo.MinVolumeInBaseCurrency}");
            Console.WriteLine($"  MinMovement: {coinInfo.MinMovement}");
            Console.WriteLine($"  MinMovementVolume: {coinInfo.MinMovementVolume}");

            // Баланс USDT
            var balance = await connector.HttpClient.GetBalance("USDT", BalanceType.Spot);
            Console.WriteLine($"\n💵 USDT Balance:");
            Console.WriteLine($"  Available: {balance.Balances.FirstOrDefault()?.AvalibleBalance}");
            Console.WriteLine($"  Frozen: {balance.Balances.FirstOrDefault()?.FreezedBalance}");

            // Последняя цена
            var lastTrade = await connector.HttpClient.GetLastTrade(instrumentName, InstrumentType.Spot);
            Console.WriteLine($"\n📊 Last Trade Price: {lastTrade?.Price}");

            // ===================================================================
            // 🎯 ОСНОВНОЙ ТЕСТ: СОЗДАНИЕ И ОТМЕНА ОРДЕРА
            // ===================================================================
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("🚀 TESTING ORDER CREATION AND CANCELLATION");
            Console.WriteLine(new string('=', 60));

            // Генерируем ClientOrderId (наш внутренний GUID)
            var clientOrderId = Guid.NewGuid().ToString("N");

            // Цена ниже рыночной (чтобы ордер не исполнился сразу)
            var orderPrice = lastTrade != null ? lastTrade.Price * 0.5m : 0.5m;

            Console.WriteLine($"\n📝 Creating order:");
            Console.WriteLine($"  ClientOrderId: {clientOrderId}");
            Console.WriteLine($"  Symbol: {instrumentName}");
            Console.WriteLine($"  Side: BUY");
            Console.WriteLine($"  Type: LIMIT");
            Console.WriteLine($"  Price: {orderPrice}");
            Console.WriteLine($"  Volume: {coinInfo.MinVolumeInBaseCurrency}");

            var newOrder = await connector.HttpClient.MakeOrderAsync(
                InstrumentType.Spot,
                OrderType.Limit,
                Direction.BUY,
                PositionOrderType.Open,
                instrumentName,
                (decimal)coinInfo.MinVolumeInBaseCurrency, // Используем минимальный объем из coinInfo
                orderPrice,
                clientOrderId);

            Console.WriteLine($"\n📋 Order Creation Result:");
            Console.WriteLine($"  Success: {newOrder.Success}");
            Console.WriteLine($"  StockOrderId (биржа): {newOrder.StockOrderId}");
            Console.WriteLine($"  SystemOrderId (наш): {newOrder.SystemOrderId}");

            if (!newOrder.Success)
            {
                Console.WriteLine($"  ❌ Error: {newOrder.Error}");
                Console.WriteLine($"  ErrorType: {newOrder.ErrorType}");
                return;
            }

            Console.WriteLine("\n⏳ Waiting 3 seconds for WebSocket order update...");
            await Task.Delay(3000);

            // ===================================================================
            // ПОЛУЧЕНИЕ ИНФОРМАЦИИ ОБ ОРДЕРЕ
            // ===================================================================
            Console.WriteLine("\n🔍 Getting order info via REST API...");
            var orderInfo = await connector.HttpClient.GetOrderInfo(
                instrumentName,
                newOrder.StockOrderId,
                InstrumentType.Spot);

            if (orderInfo != null)
            {
                Console.WriteLine($"  ✅ Order found:");
                Console.WriteLine($"    Status: {orderInfo.Status}");
                Console.WriteLine($"    Price: {orderInfo.Price}");
                Console.WriteLine($"    Volume: {orderInfo.Volume}");
                Console.WriteLine($"    FilledVolume: {orderInfo.FilledVolume}");
                Console.WriteLine($"    AvgPrice: {orderInfo.AvgPrice}");
            }
            else
            {
                Console.WriteLine($"  ❌ Order not found!");
            }

            // ===================================================================
            // ОТМЕНА ОРДЕРА
            // ===================================================================
            Console.WriteLine("\n🗑️ Canceling order...");
            var cancelResult = await connector.HttpClient.CancelOrderAsync(
                InstrumentType.Spot,
                instrumentName,
                newOrder.StockOrderId);

            Console.WriteLine($"\n📋 Cancel Result:");
            Console.WriteLine($"  Success: {cancelResult.Success}");
            if (!cancelResult.Success)
            {
                Console.WriteLine($"  ❌ Error: {cancelResult.Error}");
                Console.WriteLine($"  ErrorType: {cancelResult.ErrorType}");
            }

            Console.WriteLine("\n⏳ Waiting 3 seconds for WebSocket cancel update...");
            await Task.Delay(3000);

            // ===================================================================
            // ПРОВЕРКА СТАТУСА ПОСЛЕ ОТМЕНЫ
            // ===================================================================
            Console.WriteLine("\n🔍 Checking order status after cancel...");
            var orderInfoAfterCancel = await connector.HttpClient.GetOrderInfo(
                instrumentName,
                newOrder.StockOrderId,
                InstrumentType.Spot);

            if (orderInfoAfterCancel != null)
            {
                Console.WriteLine($"  Final Status: {orderInfoAfterCancel.Status}");
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("✅ FameEX connector test completed!");
            Console.WriteLine(new string('=', 60));

            // Держим подписки активными еще немного
            Console.WriteLine("\n⏳ Keeping connection alive for 10 more seconds...");
            await Task.Delay(10000);

            // Отключение
            await connector.HttpClient.DisconnectAsync();
            await connector.SocketClient.DisconnectAsync();
            Console.WriteLine("\n👋 Disconnected");
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
