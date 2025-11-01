using BaseStockConnector.Models.Events;
using Microsoft.Extensions.Logging;
using StockConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector
{
    public interface IStockSocketConnector
    {
        public event EventHandler Connect;
        public event EventHandler<SocketDisconectEventArgs> Disconnect;
        public Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger, WebProxy? proxy);

        public async Task ConnectAsync(IStockCredential stockCredential, ILogger clientLogger)
        {
            await ConnectAsync(stockCredential, clientLogger, null);
        }
        public Task DisconnectAsync();
    }
}
