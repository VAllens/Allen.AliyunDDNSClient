using Chimera.Extensions.Logging.Log4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Allen.AliyunDDNSClient.Ioc
{
    public class LogConfig
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddLogging();
            var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<ILoggerFactory>()
                .AddDebug()
                .AddConsole()
                .AddLog4Net(configuration.GetSection("log4net").Get<Log4NetSettings>());

            services.Add(new ServiceDescriptor(typeof(ILoggerFactory), factory));
        }
    }
}