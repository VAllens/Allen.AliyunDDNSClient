using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket.ClientEngine;

namespace Allen.AliyunDDNSClient.Ioc
{
    public class ModuleConfig
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            //AddSingleton 全局单实例
            //AddScoped 作用域内单实例
            //AddTransient 全新实例

            services.AddTransient<IClientSession, AsyncTcpSession>();

            services.AddTransient<ISearchPublicIpContext, SocketContext>();

            services.AddTransient<AliyunDdnsClient>();

            services.AddTransient<IDdns, DdnsContext>();
        }
    }
}