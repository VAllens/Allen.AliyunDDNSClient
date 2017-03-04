using System.Collections.Generic;

namespace ConsoleApp
{
    public class ConfigRoot
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