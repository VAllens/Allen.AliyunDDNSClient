namespace Allen.AliyunDDNSClient.Config.Models
{
    public class Config
    {
        /// <summary>
        /// RegionId
        /// </summary>
        public string RegionId { get; set; }

        /// <summary>
        /// Period / 调用 callback 的时间间隔（以毫秒为单位）
        /// </summary>
        public int IntervalMillisecond { get; set; }

        /// <summary>
        /// PageSize
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// AccessKeyId
        /// </summary>
        public string AccessKeyId { get; set; }

        /// <summary>
        /// AccessKeySecret
        /// </summary>
        public string AccessKeySecret { get; set; }
    }
}