using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models.Events
{
    public class SocketDisconectEventArgs
    {
        public string CloseStatusDescription { get; set; }
        public Exception Exception { get; set; }
    }
}
