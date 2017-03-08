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
using Allen.AliyunDDNSClient.Config.Models;
using Allen.AliyunDDNSClient.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allen.AliyunDDNSClient
{
    public class AliyunDdnsClient : ContextBase<DdnsClientConfig>
    {
        private const string Type = "A";

        public AliyunDdnsClient(ILogger<AliyunDdnsClient> logger, IOptions<DdnsClientConfig> options) : base(logger, options)
        {
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
            string regionId = Options.Config.RegionId;
            string accessKeyId = Options.Config.AccessKeyId;
            string accessKeySecret = Options.Config.AccessKeySecret;
            int globalPageSize = Options.Config.PageSize;
            //string domainName = "mydomain.com";
            //string rr = "www";
            //string currentIp = GetCurrentIp();

            Logger.LogDebug($"This computer's network IP address is \"{currentIp}\"");

            foreach (var domainRecord in Options.DomainRecords)
            {
                Logger.LogDebug($"Ready to update the domain name is \"{domainRecord.DomainName}\"");
                Logger.LogDebug($"The RR is \"{domainRecord.RR}\"");
                Logger.LogDebug($"Current DateTime is \"{DateTime.Now.ToLongDateTime()}\"");
                Logger.LogDebug(string.Empty);

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
                    Logger.LogDebug($"No.0 <<DescribeDomainRecordsResponse>> The request id is \"{describeDomainRecordsResponse.RequestId}\"");
                    Logger.LogDebug($"No.0 <<DescribeDomainRecordsResponse>> Total number of records matching your query is \"{describeDomainRecordsResponse.TotalCount}\"");
                    Logger.LogDebug($"No.0 <<DescribeDomainRecordsResponse>> The raw content is \"{Encoding.UTF8.GetString(describeDomainRecordsResponse.HttpResponse.Content)}\"");
                    Logger.LogDebug(string.Empty);

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
                                Logger.LogDebug($"No.{index} <<DescribeDomainRecordsResponse>> The record id is \"{item.RecordId}\", Domain Name is \"{item.DomainName}\", RR is \"{item.RR}\", value is \"{item.Value}\", does not need to be updated");
                                Logger.LogDebug(string.Empty);
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
                            Logger.LogDebug($"No.{index} <<UpdateDomainRecordRequest>> updating the domain name is \"{item.RR}.{item.DomainName}\"");
                            Logger.LogDebug($"No.{index} <<UpdateDomainRecordRequest>> Before updating the record value is \"{item.Value}\"");
                            Logger.LogDebug($"No.{index} <<UpdateDomainRecordRequest>> Updated the record value is \"{currentIp}\"");
                            Logger.LogDebug($"No.{index} <<UpdateDomainRecordRequest>> The request id is \"{updateDomainRecordResponse.RequestId}\"");
                            Logger.LogDebug($"No.{index} <<UpdateDomainRecordRequest>> The record id is \"{updateDomainRecordResponse.RecordId}\"");
                            Logger.LogDebug(string.Empty);
                        }
                    }

                    Logger.LogDebug($"Update \"{domainRecord.DomainName}\" completed!");
                    Logger.LogDebug(string.Empty);
                }
                catch (ServerException ex)
                {
                    Logger.LogDebug(0, ex, $"Throw {nameof(ServerException)}");
                }
                catch (ClientException ex)
                {
                    Logger.LogDebug(0, ex, $"Throw {nameof(ClientException)}");
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(0, ex, $"Throw {nameof(Exception)}");
                }
            }

            Logger.LogDebug("Update all completed!");
        }
    }
}