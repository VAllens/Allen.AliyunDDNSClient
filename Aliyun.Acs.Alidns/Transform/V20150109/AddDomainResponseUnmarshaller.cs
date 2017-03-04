/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using Aliyun.Acs.Core.Transform;
using Aliyun.Acs.Alidns.Model.V20150109;
using System.Collections.Generic;

namespace Aliyun.Acs.Alidns.Transform.V20150109
{
    public class AddDomainResponseUnmarshaller
    {
        public static AddDomainResponse Unmarshall(UnmarshallerContext context)
        {
            AddDomainResponse addDomainResponse = new AddDomainResponse
            {
                HttpResponse = context.HttpResponse,
                RequestId = context.StringValue("AddDomain.RequestId"),
                DomainId = context.StringValue("AddDomain.DomainId"),
                DomainName = context.StringValue("AddDomain.DomainName"),
                PunyCode = context.StringValue("AddDomain.PunyCode"),
                GroupId = context.StringValue("AddDomain.GroupId"),
                GroupName = context.StringValue("AddDomain.GroupName")
            };


            List<string> dnsServers = new List<string>();
			for (int i = 0; i < context.Length("AddDomain.DnsServers.Length"); i++) {
				dnsServers.Add(context.StringValue($"AddDomain.DnsServers[{i}]"));
			}
			addDomainResponse.DnsServers = dnsServers;
        
			return addDomainResponse;
        }
    }
}