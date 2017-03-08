namespace Allen.AliyunDDNSClient.Config.Models
{
    public class DdnsServerConfig
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public virtual int RetryCount { get; set; }

        /// <summary>
        /// 等待信号超时时间(毫秒数)
        /// </summary>
        public virtual int WaitOneMillisecondsTimeout { get; set; }

        /// <summary>
        /// Dns Server 端口号
        /// </summary>
        public virtual int? Port { get; set; }

        /// <summary>
        /// Dns Server 地址 列表
        /// </summary>
        public virtual DnsServer[] DNSServers { get; set; }
    }
}