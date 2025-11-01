using Microsoft.Extensions.Configuration;

namespace ConnectorsTester
{
    public class ChainApexConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string WebSocketUrl { get; set; } = string.Empty;

        public static ChainApexConfig Load()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var config = new ChainApexConfig();
            configuration.GetSection("ChainApex").Bind(config);

            return config;
        }
    }

    public class TestSettings
    {
        public string DefaultSymbol { get; set; } = "btcusdt";
        public string KlineInterval { get; set; } = "1min";
        public int OrderBookDepth { get; set; } = 10;
        public int TradesLimit { get; set; } = 5;
        public int KlinesLimit { get; set; } = 5;
        public int WebSocketListenSeconds { get; set; } = 10;

        public static TestSettings Load()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var settings = new TestSettings();
            configuration.GetSection("TestSettings").Bind(settings);

            return settings;
        }
    }
}

