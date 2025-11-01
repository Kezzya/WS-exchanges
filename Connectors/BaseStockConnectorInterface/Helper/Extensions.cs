using System;
using System.Collections.Generic;
using System.Text;

namespace BaseStockConnectorInterface.Helper
{
    public static class Extensions
    {
        public static long ToUnixTime(this DateTime input)
        {
            return (long)input.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long ToUnixTimeMilliseconds(this DateTime input)
        {
            return (long)input.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
