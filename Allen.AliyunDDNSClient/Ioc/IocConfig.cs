using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Allen.AliyunDDNSClient.Ioc
{
    public class IocConfig
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            OptionConfig.Configure(services, configuration);
            ModuleConfig.Configure(services, configuration);
            LogConfig.Configure(services, configuration);
        }
    }
}