using System;
using Allen.AliyunDDNSClient.Ioc;

namespace Allen.AliyunDDNSClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            Startup.Configure();

            IDdns ddns = IocManager.Instance.GetRequiredService<IDdns>();
            ddns.Start();

            Console.ReadKey();
        }
    }
}