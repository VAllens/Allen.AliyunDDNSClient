using System;

namespace Allen.AliyunDDNSClient
{
    public interface ISearchPublicIpContext /*<out TOptions>: IContext<TOptions> where TOptions : class, new()*/
    {
        event EventHandler<PublicIpEventArgs> PublicIpReceived;

        void SearchPublicIp();
    }
}