﻿//-----------------------------------------------------------------------
// <copyright file="ContentKeyAuthorizationPolicyOptionCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Represents a collection of <see cref="IContentKeyAuthorizationPolicyOption"/>.
    /// </summary>
    public class ContentKeyAuthorizationPolicyOptionCollection : CloudBaseCollection<IContentKeyAuthorizationPolicyOption>
    {
        /// <summary>
        /// The PolicyOption set name.
        /// </summary>
        internal const string ContentKeyAuthorizationPolicyOptionSet = "ContentKeyAuthorizationPolicyOptions";

        /// <summary>
        /// The media context used to communicate to the server.
        /// </summary>
        private readonly CloudMediaContext _cloudMediaContext;

        private readonly Lazy<DataServiceQuery<ContentKeyAuthorizationPolicyOptionData>> _query;


        /// <summary>
        /// Initializes a new instance of the <see cref="ContentKeyAuthorizationPolicyOptionCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal ContentKeyAuthorizationPolicyOptionCollection(CloudMediaContext cloudMediaContext)
        {
            this._cloudMediaContext = cloudMediaContext;

            this.DataContextFactory = this._cloudMediaContext.DataContextFactory;
            _query = new Lazy<DataServiceQuery<ContentKeyAuthorizationPolicyOptionData>>(()=> this.DataContextFactory.CreateDataServiceContext().CreateQuery<ContentKeyAuthorizationPolicyOptionData>(ContentKeyAuthorizationPolicyOptionSet));
        }


        /// <summary>
        /// Asynchronously creates an <see cref="IContentKeyAuthorizationPolicyOption"/> with the provided name and permissions, valid for the provided duration.
        /// </summary>
        /// <param name="name">Specifies a friendly name for the PolicyOption.</param>
        /// <param name="deliveryType">Delivery method of the content key to the client.</param>
        /// <param name="restrictions">Authorization restrictions.</param>
        /// <param name="keyDeliveryConfiguration">Xml data, specific to the key delivery type that defines how the key is delivered to the client.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;IContentKeyAuthorizationPolicyOption&gt;.</returns>
        public Task<IContentKeyAuthorizationPolicyOption> CreateAsync(
            string name, 
            ContentKeyDeliveryType deliveryType, 
            List<ContentKeyAuthorizationPolicyRestriction> restrictions,
            string keyDeliveryConfiguration)
        {
            IMediaDataServiceContext dataContext = this.DataContextFactory.CreateDataServiceContext();
            var policyOption = new ContentKeyAuthorizationPolicyOptionData
            {
                Name = name,
                Restrictions = restrictions,
                KeyDeliveryConfiguration = keyDeliveryConfiguration
            };

            ((IContentKeyAuthorizationPolicyOption)policyOption).KeyDeliveryType = deliveryType;

            policyOption.InitCloudMediaContext(this._cloudMediaContext);
            dataContext.AddObject(ContentKeyAuthorizationPolicyOptionSet, policyOption);

            return dataContext
                .SaveChangesAsync(policyOption)
                .ContinueWith<IContentKeyAuthorizationPolicyOption>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        return (ContentKeyAuthorizationPolicyOptionData)t.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Creates an PolicyOption with the provided name and permissions, valid for the provided duration.
        /// </summary>
        /// <param name="name">Specifies a friendly name for the PolicyOption.</param>
        /// <param name="deliveryType">Delivery method of the content key to the client.</param>
        /// <param name="restrictions">Authorization restrictions.</param>
        /// <param name="keyDeliveryConfiguration">Xml data, specific to the key delivery type that defines how the key is delivered to the client.</param>
        /// <returns>An <see cref="IContentKeyAuthorizationPolicyOption"/>.</returns>              
        public IContentKeyAuthorizationPolicyOption Create(
            string name,
            ContentKeyDeliveryType deliveryType,
            List<ContentKeyAuthorizationPolicyRestriction> restrictions,
            string keyDeliveryConfiguration)
        {
            try
            {
                Task<IContentKeyAuthorizationPolicyOption> task = this.CreateAsync(name, deliveryType, restrictions, keyDeliveryConfiguration);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten().InnerException;
            }
        }

            /// <summary>
        /// Gets the queryable collection of file information items.
        /// </summary>
        protected override IQueryable<IContentKeyAuthorizationPolicyOption> Queryable
        {
                get
                {
                    return this._query.Value;
                }
                set { throw new NotSupportedException(); }
        }
    }
}
