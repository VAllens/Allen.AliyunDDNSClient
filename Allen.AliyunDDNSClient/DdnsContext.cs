using System;
using System.Threading;
using System.Threading.Tasks;
using Allen.AliyunDDNSClient.Config.Models;
using Allen.AliyunDDNSClient.Extension;
using Allen.AliyunDDNSClient.Ioc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allen.AliyunDDNSClient
{
    public class DdnsContext : ContextBase<DdnsClientConfig>, IDdns, IDisposable
    {
        private Timer _timer;

        private AliyunDdnsClient Client { get; }

        private string CurrentIp { get; set; }

        private DateTime CurrentTime { get; set; }

        public DdnsContext(ILogger<DdnsContext> logger, IOptions<DdnsClientConfig> options, AliyunDdnsClient client) : base(logger, options)
        {
            Client = client;
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                //timer定时器
                _timer = new Timer((param) =>
                {
                    try
                    {
                        Logger.LogDebug("============================================================");
                        ISearchPublicIpContext socketContext = IocManager.Instance.GetRequiredService<ISearchPublicIpContext>();
                        socketContext.PublicIpReceived += SocketContext_PublicIpReceived;
                        socketContext.SearchPublicIp();
                        Logger.LogDebug("============================================================");
                        Logger.LogDebug(string.Empty);
                        Logger.LogDebug(string.Empty);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug(0, ex, "Start Method Error");
                    }
                }, this, 0, Options.Config.IntervalMillisecond);
            });
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void SocketContext_PublicIpReceived(object sender, PublicIpEventArgs e)
        {
            ISearchPublicIpContext socketContext = sender as ISearchPublicIpContext;
            if (socketContext != null)
            {
                string currentIp = e.Ip;

                DateTime startTime = DateTime.Now;
                Logger.LogDebug($"Start Time: {startTime.ToLongDateTime()}");

                if (currentIp != CurrentIp)
                {
                    Client.UpdateDomainRecord(currentIp);

                    Logger.LogDebug($"Previous Time: {CurrentTime.ToLongDateTime()}");
                    Logger.LogDebug($"Previous IP: {CurrentIp}");
                }
                else
                {
                    Logger.LogDebug("IP address need not change");
                }

                //Update成功后才能把当前IP/Time存起来
                CurrentTime = startTime;
                CurrentIp = currentIp;

                Logger.LogDebug($"Current Time: {CurrentTime.ToLongDateTime()}");
                Logger.LogDebug($"Current IP: {CurrentIp}");

                Logger.LogDebug($"End Time: {DateTime.Now.ToLongDateTime()}");
                Logger.LogDebug(string.Empty);
                Logger.LogDebug(string.Empty);
            }
        }
    }
}