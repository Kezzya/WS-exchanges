using Microsoft.Extensions.Logging;
using Serilog;
using ChainApexConnector;

namespace ConnectorsTester
{
    public static class ChainApexChainStreamTest
    {
        public static async Task TestChainStreamProtocol()
        {
            Console.WriteLine("\n=== Testing ChainStream Alternative Protocol ===");
            
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
                Console.WriteLine("\n1. Connecting to ChainStream WebSocket...");
                await connector.SocketClient.ConnectAsync(credential, logger, (System.Net.WebProxy)null!);
                Console.WriteLine("[OK] Connected to ChainStream");
                
                // Try using API key as token
                Console.WriteLine($"\n2. Trying API Key as token: {config.ApiKey}");
                
                // Try ChainStream protocol with sample channels
                var testChannels = new[]
                {
                    "dex-candle:btc_usdt_1m",
                    "dex-ticker:btc_usdt",
                    "market-stats:btc_usdt",
                    // Also try without token
                    "public:ticker"
                };
                
                // Cast to ChainApexSocketConnector to access ClientSocket
                var socketConnector = (ChainApexSocketConnector)connector.SocketClient;
                
                foreach (var channel in testChannels)
                {
                    Console.WriteLine($"\n2. Testing ChainStream channel: {channel}");
                    
                    var updateCount = 0;
                    var result = await socketConnector.ClientSocket.SpotApi.SubscribeChainStreamAsync<dynamic>(
                        channel,
                        data =>
                        {
                            updateCount++;
                            if (updateCount <= 3)
                            {
                                Console.WriteLine($"   [Update #{updateCount}] Received data on channel: {channel}");
                                Console.WriteLine($"   Data: {Newtonsoft.Json.JsonConvert.SerializeObject(data.Data)}");
                            }
                        });
                    
                    if (result.Success)
                    {
                        Console.WriteLine($"   [OK] Successfully subscribed to {channel}");
                        await Task.Delay(5000); // Listen for 5 seconds
                        Console.WriteLine($"   [INFO] Received {updateCount} updates");
                        break; // If one works, stop trying
                    }
                    else
                    {
                        Console.WriteLine($"   [FAILED] {result.Error?.Message}");
                    }
                }
                
                Console.WriteLine("\n[INFO] ChainStream protocol test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] Error during ChainStream testing: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                await connector.SocketClient.DisconnectAsync();
                Console.WriteLine("[OK] Disconnected from ChainStream WebSocket");
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

