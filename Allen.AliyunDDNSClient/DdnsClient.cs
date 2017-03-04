using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class DdnsClient
    {
        private readonly ConfigRoot _configRoot;

        private static ILogger<DdnsClient> _logger;

        private const string Type = "A";

        public DdnsClient(ConfigRoot configRoot)
        {
            _configRoot = configRoot;
            _logger = ApplicationLogging.CreateLogger<DdnsClient>();
        }

        public string GetCurrentIp()
        {
            var httpClient = new HttpClient(new HttpClientHandler
                                            {
                                                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None,
                                            });
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("aliyun-ddns-client-csharp")));

            var htmlSource = httpClient.GetStringAsync("http://ip.chinaz.com").Result;
            var ip = Regex.Match(htmlSource, "(?<=<dd class=\"fz24\">)[\\d\\.]+(?=</dd>)", RegexOptions.IgnoreCase).Value;

            return ip;
        }

        public void UpdateDomainRecord(string currentIp)
        {
            //基础参数
            string regionId = _configRoot.Config.RegionId;
            string accessKeyId = _configRoot.Config.AccessKeyId;
            string accessKeySecret = _configRoot.Config.AccessKeySecret;
            int globalPageSize = _configRoot.Config.PageSize;
            //string domainName = "mydomain.com";
            //string rr = "www";
            //string currentIp = GetCurrentIp();

            WriteLog($"This computer's network IP address is \"{currentIp}\"");

            foreach (var domainRecord in _configRoot.DomainRecords)
            {
                WriteLog($"Ready to update the domain name is \"{domainRecord.DomainName}\"");
                WriteLog($"The RR is \"{domainRecord.RR}\"");
                WriteLog($"Current DateTime is \"{DateTime.Now.ToLongDateTime()}\"");
                WriteLog(string.Empty);

                try
                {
                    int pageSize = domainRecord.PageSize > 0 ? domainRecord.PageSize : globalPageSize;
                    //初始化默认的ACS客户端
                    IClientProfile clientProfile = DefaultProfile.GetProfile(regionId, accessKeyId, accessKeySecret);
                    DefaultAcsClient client = new DefaultAcsClient(clientProfile);

                    //初始化指定域名的解析记录信息查询请求
                    DescribeDomainRecordsRequest describeDomainRecordsRequest = new DescribeDomainRecordsRequest
                                                                                {
                                                                                    PageNumber = 1,
                                                                                    PageSize = pageSize,
                                                                                    RRKeyWord = domainRecord.RR,
                                                                                    TypeKeyWord = Type,
                                                                                    DomainName = domainRecord.DomainName
                                                                                    //AcceptFormat = FormatType.JSON
                                                                                };
                    //执行并且获取查询指定域名的解析记录信息的响应结果
                    DescribeDomainRecordsResponse describeDomainRecordsResponse = client.GetAcsResponse(describeDomainRecordsRequest);
                    WriteLog($"No.0 <<DescribeDomainRecordsResponse>> The request id is \"{describeDomainRecordsResponse.RequestId}\"");
                    WriteLog($"No.0 <<DescribeDomainRecordsResponse>> Total number of records matching your query is \"{describeDomainRecordsResponse.TotalCount}\"");
                    WriteLog($"No.0 <<DescribeDomainRecordsResponse>> The raw content is \"{Encoding.UTF8.GetString(describeDomainRecordsResponse.HttpResponse.Content)}\"");
                    WriteLog(string.Empty);

                    if (describeDomainRecordsResponse.TotalCount.HasValue && describeDomainRecordsResponse.TotalCount.Value > 0 && describeDomainRecordsResponse.DomainRecords.Any())
                    {
                        //待更新的解析记录总列表
                        var domainRecords = describeDomainRecordsResponse.DomainRecords;

                        //总页数
                        int totalPage = Convert.ToInt32(describeDomainRecordsResponse.TotalCount.Value / pageSize);
                        if ((describeDomainRecordsResponse.TotalCount.Value % pageSize) > 0)
                        {
                            totalPage++;
                        }

                        //当总页数大于1时，表示还有下一页
                        if (totalPage > 1)
                        {
                            //从第二页开始获取待更新的解析记录列表
                            for (int pageNumber = 2; pageNumber <= totalPage; pageNumber++)
                            {
                                describeDomainRecordsRequest.PageNumber = pageNumber;
                                describeDomainRecordsResponse = client.GetAcsResponse(describeDomainRecordsRequest);
                                if (describeDomainRecordsResponse.DomainRecords.Any())
                                {
                                    //添加到待解析记录总列表
                                    domainRecords.AddRange(describeDomainRecordsResponse.DomainRecords);
                                }
                            }
                        }

                        //更新指定域名的解析记录值
                        int index = 0;
                        foreach (var item in domainRecords)
                        {
                            index++;
                            if (item.Value == currentIp)
                            {
                                WriteLog($"No.{index} <<DescribeDomainRecordsResponse>> The record id is \"{item.RecordId}\", Domain Name is \"{item.DomainName}\", RR is \"{item.RR}\", value is \"{item.Value}\", does not need to be updated");
                                WriteLog(string.Empty);
                                continue;
                            }

                            //初始化指定域名的解析记录更新请求
                            UpdateDomainRecordRequest updateDomainRecordRequest = new UpdateDomainRecordRequest
                                                                                  {
                                                                                      RecordId = item.RecordId,
                                                                                      RR = item.RR,
                                                                                      Type = item.Type,
                                                                                      Value = currentIp
                                                                                  };
                            //执行并且获取更新指定域名的解析记录的响应结果
                            UpdateDomainRecordResponse updateDomainRecordResponse = client.GetAcsResponse(updateDomainRecordRequest);
                            WriteLog($"No.{index} <<UpdateDomainRecordRequest>> updating the domain name is \"{item.RR}.{item.DomainName}\"");
                            WriteLog($"No.{index} <<UpdateDomainRecordRequest>> Before updating the record value is \"{item.Value}\"");
                            WriteLog($"No.{index} <<UpdateDomainRecordRequest>> Updated the record value is \"{currentIp}\"");
                            WriteLog($"No.{index} <<UpdateDomainRecordRequest>> The request id is \"{updateDomainRecordResponse.RequestId}\"");
                            WriteLog($"No.{index} <<UpdateDomainRecordRequest>> The record id is \"{updateDomainRecordResponse.RecordId}\"");
                            WriteLog(string.Empty);
                        }
                    }

                    WriteLog($"Update \"{domainRecord.DomainName}\" completed!");
                    WriteLog(string.Empty);
                }
                catch (ServerException e)
                {
                    WriteLog(e);
                }
                catch (ClientException e)
                {
                    WriteLog(e);
                }
                catch (Exception e)
                {
                    WriteLog(e);
                }
            }

            WriteLog("Update all completed!");
        }

        private static void WriteLog(string message, params object[] args)
        {
            EventId eventId = new EventId(2, nameof(DdnsClient));
            _logger.LogDebug(eventId, message, args);
        }

        private static void WriteLog(Exception ex, params object[] args)
        {
            EventId eventId = new EventId(2, nameof(DdnsClient));
            _logger.LogDebug(eventId, ex, ex.Message, args);
        }
    }
}