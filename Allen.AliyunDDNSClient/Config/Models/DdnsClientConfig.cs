using System.Collections.Generic;

namespace Allen.AliyunDDNSClient.Config.Models
{
    public class DdnsClientConfig
    {
        /// <summary>
        /// Config
        /// </summary>
        public Config Config { get; set; }

        /// <summary>
        /// DomainRecords
        /// </summary>
        public List<DomainRecord> DomainRecords { get; set; }
    }
}