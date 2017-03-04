using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class DdnsContext
    {
        private static Timer _timer;

        private static ILogger<DdnsContext> _logger;

        private static ConfigRoot _config;

        private static DdnsContext _context;

        public static DdnsContext Instance
        {
            get
            {
                if (_context == null)
                {
                    IConfigReader reader = new JsonConfigReader();
                    _config = reader.Read();

                    _logger = ApplicationLogging.CreateLogger<DdnsContext>();

                    _context = new DdnsContext()
                    {
                        DueTime = 0,
                        IntervalMillisecond = _config.Config.IntervalMillisecond,
                        Client = new DdnsClient(_config)
                    };
                }

                return _context;
            }
        }

        public DdnsClient Client { get; set; }

        internal string CurrentIp { get; set; }

        internal DateTime CurrentTime { get; set; }

        /// <summary>
        /// 调用 callback 之前延迟的时间量（以毫秒为单位）
        /// </summary>
        internal int DueTime { get; set; }

        /// <summary>
        /// Period / 调用 callback 的时间间隔（以毫秒为单位）
        /// </summary>
        internal int IntervalMillisecond { get; set; }

        public static void Run()
        {
            Task.Run(() =>
            {
                //timer定时器
                _timer = new Timer((param) =>
                {
                    try
                    {
                        WriteLog("============================================================");
                        SocketContext socketContext = new SocketContext();
                        socketContext.PublicIpReceived += SocketContext_PublicIpReceived;
                        socketContext.SearchPublicIp();
                        WriteLog("============================================================");
                        WriteLog(string.Empty);
                        WriteLog(string.Empty);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                    }
                }, Instance, Instance.DueTime, Instance.IntervalMillisecond);
            });
        }

        private static void SocketContext_PublicIpReceived(object sender, PublicIpEventArgs e)
        {
            SocketContext socketContext = sender as SocketContext;
            if (socketContext != null)
            {
                DdnsContext context = Instance;
                DdnsClient client = context.Client;
                string currentIp = e.Ip;

                DateTime startTime = DateTime.Now;
                WriteLog($"Start Time: {startTime.ToLongDateTime()}");

                if (currentIp != context.CurrentIp)
                {
                    client.UpdateDomainRecord(currentIp);

                    WriteLog($"Previous Time: {context.CurrentTime.ToLongDateTime()}");
                    WriteLog($"Previous IP: {context.CurrentIp}");
                }
                else
                {
                    WriteLog("IP address need not change");
                }

                //Update成功后才能把当前IP/Time存起来
                context.CurrentTime = startTime;
                context.CurrentIp = currentIp;

                WriteLog($"Current Time: {context.CurrentTime.ToLongDateTime()}");
                WriteLog($"Current IP: {context.CurrentIp}");

                DateTime endTime = DateTime.Now;
                WriteLog($"End Time: {endTime.ToLongDateTime()}");
                WriteLog(string.Empty);
                WriteLog(string.Empty);
            }
        }

        private static void WriteLog(string message, params object[] args)
        {
            EventId eventId = new EventId(1, nameof(DdnsContext));
            _logger.LogDebug(eventId, message, args);
        }

        private static void WriteLog(Exception ex, params object[] args)
        {
            EventId eventId = new EventId(1, nameof(DdnsContext));
            _logger.LogDebug(eventId, ex, ex.Message, args);
        }
    }
}