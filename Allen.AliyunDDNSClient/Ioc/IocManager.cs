using System;
using Microsoft.Extensions.DependencyInjection;

namespace Allen.AliyunDDNSClient.Ioc
{
    public class IocManager
    {
        public static IocManager Instance { get; set; }

        private readonly IServiceProvider _services;

        public IocManager(IServiceCollection services)
        {
            _services = services.BuildServiceProvider();
        }

        /// <summary>
        /// 得到服务对象实例，可返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>()
        {
            return _services.GetService<T>();
        }

        /// <summary>
        /// 得到服务对象实例，如果为null，抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRequiredService<T>()
        {
            return _services.GetRequiredService<T>();
        }
    }
}