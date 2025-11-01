using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStockConnectorInterface.Helper
{
    public class RateLimitExceededMessage
    {
        public string? Exchange { get; set; }

        public string? Account { get; set; }

        public string? TaskId { get; set; }

        public string? Proxy { get; set; }

        public string? RequestPath { get; set; }

        public string Description { get; set; }

        public int Current { get; set; }

        public int RequestWeight { get; set; }

        public string RateLimitBehaviour { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return
                $"[{Timestamp} UTC][{Exchange}][Acc: {Account}|Task: {TaskId}]\n" +
                $"RL hit!\n" +
                $"|Proxy: {Proxy}" +
                $"|Path: {RequestPath}" +
                $"|Description: {Description}" +
                $"|Current: {Current}" +
                $"|ReqWeight: {RequestWeight}" +
                $"|RateLimitBehaviour: {RateLimitBehaviour}";

        }
    }
}
