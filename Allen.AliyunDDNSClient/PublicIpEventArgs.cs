using System;

namespace Allen.AliyunDDNSClient
{
    /// <summary>
    /// 公网IP事件数据
    /// </summary>
    public class PublicIpEventArgs : EventArgs
    {
        public string Ip { get; set; }
    }
}