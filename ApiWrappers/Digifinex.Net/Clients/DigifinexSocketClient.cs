using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using Digifinex.Net.Objects.Options;
using Digifinex.Net.Clients.SpotApi;
using Microsoft.Extensions.Logging;

namespace Digifinex.Net.Clients
{
    public class DigifinexSocketClient : BaseSocketClient
    {
        public DigifinexSocketClientSpotApi SpotApi { get; set; }

        #region ctor

        /// <summary>
        /// Create a new instance of the DigifinexSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        public DigifinexSocketClient(ILoggerFactory? loggerFactory = null) : this((x) => { }, loggerFactory)
        {
        }

        /// <summary>
        /// Create a new instance of the DigifinexSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public DigifinexSocketClient(Action<DigifinexSocketOptions> optionsDelegate) : this(optionsDelegate, null)
        {
        }

        /// <summary>
        /// Create a new instance of the DigifinexSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public DigifinexSocketClient(Action<DigifinexSocketOptions> optionsDelegate, ILoggerFactory? loggerFactory = null) : base(loggerFactory, "Digifinex")
        {
            var options = DigifinexSocketOptions.Default.Copy();
            optionsDelegate(options);
            Initialize(options);

            SpotApi = AddApiClient(new DigifinexSocketClientSpotApi(_logger, options));
        }

        #endregion ctor

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<DigifinexSocketOptions> optionsDelegate)
        {
            var options = DigifinexSocketOptions.Default.Copy();
            optionsDelegate(options);
            DigifinexSocketOptions.Default = options;
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
        }
    }
}