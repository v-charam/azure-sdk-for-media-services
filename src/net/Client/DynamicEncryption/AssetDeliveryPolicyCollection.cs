﻿//-----------------------------------------------------------------------
// <copyright file="AssetDeliveryPolicyCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption
{
    /// <summary>
    /// Represents a collection of <see cref="IAssetDeliveryPolicy"/>.
    /// </summary>
    public class AssetDeliveryPolicyCollection : CloudBaseCollection<IAssetDeliveryPolicy>
    {
        /// <summary>
        /// The PolicyOption set name.
        /// </summary>
        internal const string DeliveryPolicySet = "AssetDeliveryPolicies";

        /// <summary>
        /// The media context used to communicate to the server.
        /// </summary>
        private readonly CloudMediaContext _cloudMediaContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDeliveryPolicyCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal AssetDeliveryPolicyCollection(CloudMediaContext cloudMediaContext)
        {
            this._cloudMediaContext = cloudMediaContext;

            this.DataContextFactory = this._cloudMediaContext.MediaServicesClassFactory;
            this.Queryable = this.DataContextFactory.CreateDataServiceContext().CreateQuery<AssetDeliveryPolicyData>(DeliveryPolicySet);
        }
 
        /// <summary>
        /// Asynchronously creates an <see cref="IAssetDeliveryPolicy"/>.
        /// </summary>
        /// <param name="name">Friendly name for the policy.</param>
        /// <param name="policyType">Type of the policy.</param>
        /// <param name="deliveryProtocol">Delivery protocol.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>An <see cref="IAssetDeliveryPolicy"/>.</returns>
        public Task<IAssetDeliveryPolicy> CreateAsync(
            string name,
            AssetDeliveryPolicyType policyType,
            AssetDeliveryProtocol deliveryProtocol,
            Dictionary<AssetDeliveryPolicyConfigurationKey, string> configuration)
        {
            IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            var policy = new AssetDeliveryPolicyData
            {
                Name = name, 
            };

            ((IAssetDeliveryPolicy)policy).AssetDeliveryPolicyType = policyType;
            ((IAssetDeliveryPolicy)policy).AssetDeliveryProtocol = deliveryProtocol;
            ((IAssetDeliveryPolicy)policy).AssetDeliveryConfiguration = configuration;

            policy.InitCloudMediaContext(this._cloudMediaContext);
            dataContext.AddObject(DeliveryPolicySet, policy);

            return dataContext
                .SaveChangesAsync(policy)
                .ContinueWith<IAssetDeliveryPolicy>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        return (AssetDeliveryPolicyData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Creates a delivery policy.
        /// </summary>
        /// <param name="name">Friendly name for the policy.</param>
        /// <param name="policyType">Type of the policy.</param>
        /// <param name="deliveryProtocol">Delivery protocol.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>An <see cref="IAssetDeliveryPolicy"/>.</returns>
        public IAssetDeliveryPolicy Create(
            string name,
            AssetDeliveryPolicyType policyType,
            AssetDeliveryProtocol deliveryProtocol,
            Dictionary<AssetDeliveryPolicyConfigurationKey, string> configuration)
        {
            try
            {
                Task<IAssetDeliveryPolicy> task = this.CreateAsync(name, policyType, deliveryProtocol, configuration);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }
    }
}
