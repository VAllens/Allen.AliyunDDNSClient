using System;

namespace Allen.AliyunDDNSClient.Extension
{
    public static class DateTimeExtensions
    {
        public static string ToLongDateTime(this DateTime time, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return time.ToString(format);
        }
    }
}