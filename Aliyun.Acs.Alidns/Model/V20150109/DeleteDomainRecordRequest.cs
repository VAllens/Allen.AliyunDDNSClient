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
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Transform;
using Aliyun.Acs.Core.Utils;
using Aliyun.Acs.Alidns.Transform.V20150109;

namespace Aliyun.Acs.Alidns.Model.V20150109
{
    public class DeleteDomainRecordRequest : RpcAcsRequest<DeleteDomainRecordResponse>
    {
        public DeleteDomainRecordRequest()
            : base("Alidns", "2015-01-09", "DeleteDomainRecord")
        {
        }

		private string _lang;

		private string _userClientIp;

		private string _recordId;

		public string Lang
		{
			get
			{
				return _lang;
			}
			set	
			{
				_lang = value;
				DictionaryUtil.Add(QueryParameters, "Lang", value);
			}
		}

		public string UserClientIp
		{
			get
			{
				return _userClientIp;
			}
			set	
			{
				_userClientIp = value;
				DictionaryUtil.Add(QueryParameters, "UserClientIp", value);
			}
		}

		public string RecordId
		{
			get
			{
				return _recordId;
			}
			set	
			{
				_recordId = value;
				DictionaryUtil.Add(QueryParameters, "RecordId", value);
			}
		}

        public override DeleteDomainRecordResponse GetResponse(UnmarshallerContext unmarshallerContext)
        {
            return DeleteDomainRecordResponseUnmarshaller.Unmarshall(unmarshallerContext);
        }
    }
}