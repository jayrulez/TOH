using System;

namespace TOH.Common.Utilities
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1);

            var timeSpan = dateTime - epoch;

            return (long)timeSpan.TotalSeconds;
        }
    }
}
