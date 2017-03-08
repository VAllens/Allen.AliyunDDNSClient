using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allen.AliyunDDNSClient
{
    public abstract class ContextBase<TOptions> where TOptions : class, new()
    {
        protected ILogger<ContextBase<TOptions>> Logger { get; }

        protected TOptions Options { get; }

        protected ContextBase(ILogger<ContextBase<TOptions>> logger, IOptions<TOptions> options)
        {
            Logger = logger;
            Options = options.Value;
        }
    }
}