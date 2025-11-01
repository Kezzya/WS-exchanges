using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnector.Models
{
    public class SubscriptionResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public T OriginalObject { get; set; }
    }
}
