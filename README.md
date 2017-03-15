# Allen.AliyunDDNSClient

这是一个DDNS客户端，定时获取当前运行环境的外网IP，然后更新到阿里云的域名解析服务。
作用是令你的域名，保持最新解析状态。

它可运行于任何支持.net core的环境，例如Linux，Windows。
当然了，docker也是支持的。
因为作者本人已经把它部署在Synology NAS环境(DSM Docker)。