using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using FameEX.Net.Clients.SpotApi;
using FameEX.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FameEX.Net.Clients
{
    public class FameexSocketClient : BaseSocketClient
    {
        public FameexSocketClientSpotApi SpotApi { get; set; }

        #region ctor

        /// <summary>
        /// Create a new instance of the FameexSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        public FameexSocketClient(ILoggerFactory? loggerFactory = null) : this((x) => { }, loggerFactory)
        {
        }

        /// <summary>
        /// Create a new instance of the FameexSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public FameexSocketClient(Action<FameexSocketOptions> optionsDelegate) : this(optionsDelegate, null)
        {
        }

        /// <summary>
        /// Create a new instance of the FameexSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <param name="loggerFactory">The logger factory</param>
        public FameexSocketClient(Action<FameexSocketOptions> optionsDelegate, ILoggerFactory? loggerFactory = null) : base(loggerFactory, "FameEX")
        {
            var options = FameexSocketOptions.Default.Copy();
            optionsDelegate(options);
            
            Initialize(options);

            SpotApi = AddApiClient(new FameexSocketClientSpotApi(_logger, options));
        }

        #endregion ctor

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<FameexSocketOptions> optionsDelegate)
        {
            var options = FameexSocketOptions.Default.Copy();
            optionsDelegate(options);
            FameexSocketOptions.Default = options;
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
        }
    }
}