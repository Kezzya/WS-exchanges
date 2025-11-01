using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using ChainApex.Net.Objects.Options;
using ChainApex.Net.Clients.SpotApi;
using Microsoft.Extensions.Logging;

namespace ChainApex.Net.Clients
{
    public class ChainApexSocketClient : BaseSocketClient
    {
        public ChainApexSocketClientSpotApi SpotApi { get; set; }

        #region ctor

        /// <summary>
        /// Create a new instance of the ChainApexSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        public ChainApexSocketClient(ILoggerFactory? loggerFactory = null) : this((x) => { }, loggerFactory)
        {
        }

        /// <summary>
        /// Create a new instance of the ChainApexSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public ChainApexSocketClient(Action<ChainApexSocketOptions> optionsDelegate) : this(optionsDelegate, null)
        {
        }

        /// <summary>
        /// Create a new instance of the ChainApexSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public ChainApexSocketClient(Action<ChainApexSocketOptions> optionsDelegate, ILoggerFactory? loggerFactory = null) : base(loggerFactory, "ChainApex")
        {
            var options = ChainApexSocketOptions.Default.Copy();
            optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new ChainApexSocketClientSpotApi(_logger, options.Environment.PublicSocketClientAddress, options));
        }

        #endregion ctor

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<ChainApexSocketOptions> optionsDelegate)
        {
            var options = ChainApexSocketOptions.Default.Copy();
            optionsDelegate(options);
            ChainApexSocketOptions.Default = options;
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
        }
    }
}
