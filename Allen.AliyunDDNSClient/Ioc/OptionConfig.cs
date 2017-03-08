using System.IO;
using Allen.AliyunDDNSClient.Config.Models;
using Chimera.Extensions.Logging.Log4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Allen.AliyunDDNSClient.Ioc
{
    public class OptionConfig
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions();

            var setting1 = configuration.GetSection("DdnsServer").Get<Log4NetSettings>();
            var ddnsServerConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(setting1.ConfigFilePath, false, setting1.Watch)
                .Build();
            services.Configure<DdnsServerConfig>(ddnsServerConfig);

            var setting2 = configuration.GetSection("DdnsClient").Get<Log4NetSettings>();
            var ddnsClientConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(setting2.ConfigFilePath, false, setting2.Watch)
                .Build();
            services.Configure<DdnsClientConfig>(ddnsClientConfig);
        }
    }
}