using System.IO;
using Allen.AliyunDDNSClient.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Allen.AliyunDDNSClient
{
    public class Startup
    {
        public static void Configure()
        {
            IServiceCollection services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            IocConfig.Configure(services, configuration);

            IocManager.Instance = new IocManager(services);
        }
    }
}