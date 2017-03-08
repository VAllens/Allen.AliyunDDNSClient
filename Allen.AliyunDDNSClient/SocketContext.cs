using System;
using System.Net;
using System.Text;
using System.Threading;
using Allen.AliyunDDNSClient.Config.Models;
using Allen.AliyunDDNSClient.Ioc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.ClientEngine;

namespace Allen.AliyunDDNSClient
{
    public class SocketContext : ContextBase<DdnsServerConfig>, ISearchPublicIpContext
    {
        /// <summary>
        /// 线程同步对象
        /// </summary>
        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(false);

        /// <summary>
        /// 是否退出循环
        /// </summary>
        private bool _isBreak;

        /// <summary>
        /// 公网IP事件订阅
        /// </summary>
        public virtual event EventHandler<PublicIpEventArgs> PublicIpReceived;

        public SocketContext(ILogger<SocketContext> logger, IOptions<DdnsServerConfig> options) : base(logger, options)
        {
        }

        /// <summary>
        /// 执行查询公网IP操作
        /// </summary>
        public void SearchPublicIp()
        {
            Logger.LogDebug($"Execute {nameof(SearchPublicIp)} method begin.");

            if (!Options.Port.HasValue)
            {
                Options.Port = 6666;
            }

            foreach (var server in Options.DNSServers)
            {
                if (!server.Port.HasValue)
                {
                    server.Port = Options.Port;
                }
                var endPoint = new DnsEndPoint(server.Ip, server.Port.Value); //公网IP查询的Socket服务器地址

                //IClientSession session = CreateAsyncTcpSession(); //初始化一个异步的TCP连接对象

                ////连接服务器，并且执行公网IP查询操作
                //session.Connect(endPoint);

                ////等待接收终止状态信号，一旦接收到终止信号，将继续往下执行
                //_waitHandle.WaitOne(Options.WaitOneMillisecondsTimeout);

                ////当_isBreak为true时，表示成功获取到公网IP地址，无须继续foreach遍历，即退出当前foreach遍历。
                //if (_isBreak)
                //{
                //    break;
                //}

                //当代码执行到此处时，表示_isBreak为false，上面没有成功从server获取到公网IP地址，需要重试retryCount次。
                //当重试retryCount次后，_isBreak依旧为false，则继续foreach遍历，获取下一个server地址，继续查询。
                //直到获取成功，_isBreak为true，退出foreach遍历。
                int retryCount = 1; //重试获取公网IP的执行次数
                do
                {
                    IClientSession session = CreateAsyncTcpSession(); //初始化一个异步的TCP连接对象

                    //连接服务器，并且执行公网IP查询操作
                    session.Connect(endPoint);

                    //等待接收终止状态信号，一旦接收到终止信号，将继续往下执行
                    _waitHandle.WaitOne(Options.WaitOneMillisecondsTimeout);

                    //当_isBreak为true时，表示成功获取到公网IP地址，无须继续while重试，即退出当前while循环。
                    if (_isBreak)
                    {
                        break;
                    }

                    retryCount++;
                } while (Options.RetryCount > retryCount);

                //当_isBreak为false时，表示上面的while重试了retryCount次，依旧没有获取成功，所以继续foreach遍历。
                //反之退出foreach遍历。
                if (_isBreak)
                {
                    break;
                }
            }

            Logger.LogDebug($"Execute {nameof(SearchPublicIp)} method end.");
        }

        private IClientSession CreateAsyncTcpSession()
        {
            IClientSession session = IocManager.Instance.GetRequiredService<IClientSession>();
            session.ReceiveBufferSize = 16;
            session.Connected += Connected;
            session.Error += Error;

            return session;
        }

        private void Connected(object sender, EventArgs e)
        {
            Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(Connected)} event begin.");

            var clientSession = sender as IClientSession;
            if (clientSession != null)
            {
                clientSession.DataReceived += DataReceived;
            }

            Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(Connected)} event end.");
        }

        private void Error(object sender, ErrorEventArgs e)
        {
            Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(Error)} event begin.");

            var clientSession = sender as IClientSession;
            if (clientSession != null)
            {
                var ex = e.Exception;

                Logger.LogDebug(0, ex, "Socket connection error.");
            }

            _isBreak = false;
            _waitHandle.Set();

            Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(Error)} event end.");
        }

        private void DataReceived(object sender, DataEventArgs e)
        {
            Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(DataReceived)} event begin.");

            var clientSession = sender as IClientSession;
            if (clientSession != null)
            {
                clientSession.Close();

                if (PublicIpReceived != null)
                {
                    byte[] data = e.Data;
                    string ipString = Encoding.UTF8.GetString(data, 0, 16).Replace("\0", string.Empty);

                    var args = new PublicIpEventArgs
                               {
                                   Ip = ipString
                               };

                    try
                    {
                        Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(PublicIpReceived)} event begin.");

                        PublicIpReceived.Invoke(this, args);

                        Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(PublicIpReceived)} event end.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug(0, ex, "PublicIpReceived event handler error.");
                    }
                }
            }

            _isBreak = true;
            _waitHandle.Set();

            Logger.LogDebug($"Trigger <<{nameof(IClientSession)}>> {nameof(DataReceived)} event end.");
        }
    }
}