using System;
using System.Net;
using System.Text;
using System.Threading;
using SuperSocket.ClientEngine;

namespace ConsoleApp
{
    public class SocketContext
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        private static readonly int RetryCount = 5;

        /// <summary>
        /// 等待信号超时时间(毫秒数)
        /// </summary>
        private static readonly int WaitOneMillisecondsTimeout = 5 * 60 * 1000; //分*秒*毫秒

        /// <summary>
        /// Dns Server 端口号
        /// </summary>
        private static readonly int Port = 6666;

        /// <summary>
        /// Dns Server 列表
        /// </summary>
        private static readonly string[] DNSServers = {"ns1.dnspod.net", "ns2.dnspod.net", "ns3.dnspod.net", "ns4.dnspod.net", "ns5.dnspod.net", "ns6.dnspod.net"};

        /// <summary>
        /// 线程同步对象
        /// </summary>
        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(false);

        /// <summary>
        /// 是否退出循环
        /// </summary>
        private bool _isBreak = true;

        /// <summary>
        /// 公网IP事件订阅
        /// </summary>
        public event EventHandler<PublicIpEventArgs> PublicIpReceived;

        /// <summary>
        /// 执行查询公网IP操作
        /// </summary>
        public void SearchPublicIp()
        {
            Console.WriteLine($"Execute {nameof(SearchPublicIp)} method begin.");

            foreach (var server in DNSServers)
            {
                var endPoint = new DnsEndPoint(server, Port); //公网IP查询的Socket服务器地址

                IClientSession session = CreateAsyncTcpSession(); //初始化一个异步的TCP连接对象

                //连接服务器，并且执行公网IP查询操作
                session.Connect(endPoint);

                //等待接收终止状态信号，一旦接收到终止信号，将继续往下执行
                _waitHandle.WaitOne(WaitOneMillisecondsTimeout);

                //当_isBreak为true时，表示成功获取到公网IP地址，无须继续foreach遍历，即退出当前foreach遍历。
                if (_isBreak)
                {
                    break;
                }

                //当代码执行到此处时，表示_isBreak为false，上面没有成功从server获取到公网IP地址，需要重试retryCount次。
                //当重试retryCount次后，_isBreak依旧为false，则继续foreach遍历，获取下一个server地址，继续查询。
                //直到获取成功，_isBreak为true，退出foreach遍历。
                int retryCount = 0; //重试获取公网IP次数
                while (RetryCount > retryCount)
                {
                    retryCount++;

                    session = CreateAsyncTcpSession(); //初始化一个异步的TCP连接对象

                    //连接服务器，并且执行公网IP查询操作
                    session.Connect(endPoint);

                    //等待接收终止状态信号，一旦接收到终止信号，将继续往下执行
                    _waitHandle.WaitOne(WaitOneMillisecondsTimeout);

                    //当_isBreak为true时，表示成功获取到公网IP地址，无须继续while重试，即退出当前while循环。
                    if (_isBreak)
                    {
                        break;
                    }
                }

                //当_isBreak为false时，表示上面的while重试了retryCount次，依旧没有获取成功，所以继续foreach遍历。
                //反之退出foreach遍历。
                if (_isBreak)
                {
                    break;
                }
            }

            Console.WriteLine($"Execute {nameof(SearchPublicIp)} method end.");
        }

        private IClientSession CreateAsyncTcpSession()
        {
            AsyncTcpSession session = new AsyncTcpSession()
                                      {
                                          ReceiveBufferSize = 16
                                      };

            session.Connected += Connected;
            session.Error += Error;

            return session;
        }

        private void Connected(object sender, EventArgs e)
        {
            Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(Connected)} event begin.");

            var clientSession = sender as IClientSession;
            if (clientSession != null)
            {
                clientSession.DataReceived += DataReceived;
            }

            Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(Connected)} event end.");
        }

        private void Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(Error)} event begin.");

            var clientSession = sender as IClientSession;
            if (clientSession != null)
            {
                var ex = e.Exception;

                Console.WriteLine(ex);
                throw ex;
            }

            _isBreak = false;
            _waitHandle.Set();

            Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(Error)} event end.");
        }

        private void DataReceived(object sender, DataEventArgs e)
        {
            Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(DataReceived)} event begin.");

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
                        Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(PublicIpReceived)} event begin.");

                        PublicIpReceived.Invoke(this, args);

                        Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(PublicIpReceived)} event end.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            _isBreak = true;
            _waitHandle.Set();

            Console.WriteLine($"Trigger <<{nameof(IClientSession)}>> {nameof(DataReceived)} event end.");
        }
    }
}