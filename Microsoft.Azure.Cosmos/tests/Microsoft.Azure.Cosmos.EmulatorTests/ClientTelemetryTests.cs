﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.SDK.EmulatorTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.Azure.Cosmos.Tracing;
    using Microsoft.Azure.Cosmos.Telemetry;
    using System.IO;
    using System.Diagnostics;
    using System.Linq;

    [TestClass]
    public class ClientTelemetryTests : BaseCosmosClientHelper
    {
        private const string telemetryEndpointUrl = "http://dummy.telemetry.endpoint/";
        private const int scheduledInSeconds = 1;
        private CosmosClientBuilder cosmosClientBuilder;

        private List<ClientTelemetryProperties> actualInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            this.actualInfo = new List<ClientTelemetryProperties>();

            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, "1");
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetryEndpoint, telemetryEndpointUrl);

            HttpClientHandlerHelper httpHandler = new HttpClientHandlerHelper
            {
                RequestCallBack = (request, cancellation) =>
                {
                    if (request.RequestUri.AbsoluteUri.Equals(ClientTelemetryOptions.GetClientTelemetryEndpoint().AbsoluteUri))
                    {
                        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

                        string jsonObject = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        lock (this.actualInfo)
                        {
                            this.actualInfo.Add(JsonConvert.DeserializeObject<ClientTelemetryProperties>(jsonObject));
                        }

                        return Task.FromResult(result);
                    }
                    else if (request.RequestUri.AbsoluteUri.Equals(ClientTelemetryOptions.GetVmMetadataUrl().AbsoluteUri))
                    {
                        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

                        object jsonObject = JsonConvert.DeserializeObject("{\"compute\":{\"azEnvironment\":\"AzurePublicCloud\",\"customData\":\"\",\"isHostCompatibilityLayerVm\":\"false\",\"licenseType\":\"\",\"location\":\"eastus\",\"name\":\"sourabh-testing\",\"offer\":\"UbuntuServer\",\"osProfile\":{\"adminUsername\":\"azureuser\",\"computerName\":\"sourabh-testing\"},\"osType\":\"Linux\",\"placementGroupId\":\"\",\"plan\":{\"name\":\"\",\"product\":\"\",\"publisher\":\"\"},\"platformFaultDomain\":\"0\",\"platformUpdateDomain\":\"0\",\"provider\":\"Microsoft.Compute\",\"publicKeys\":[{\"keyData\":\"ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC5uCeOAm3ehmhI+2PbMoMl17Eo\r\nqfHKCycSaBJsv9qxlmBOuFheSJc1XknJleXUSsuTO016/d1PyWpevnqOZNRksWoa\r\nJvQ23sDTxcK+X2OP3QlCUeX4cMjPXqlL8z1UYzU4Bx3fFvf8fs67G3N72sxWBw5P\r\nZyuXyhBm0NCe/2NYMKgEDT4ma8XszO0ikbhoPKbMbgHAQk/ktWQHNcqYOPQKEWqp\r\nEK1R0rjS2nmtovfScP/ZGXcvOpJ1/NDBo4dh1K+OxOGM/4PSH/F448J5Zy4eAyEk\r\nscys+IpeIOTOlRUy/703SNIX0LEWlnYqbyL9c1ypcYLQqF76fKkDfzzFI/OWVlGw\r\nhj/S9uP8iMsR+fhGIbn6MAa7O4DWPWLuedSp7KDYyjY09gqNJsfuaAJN4LiC6bPy\r\nhknm0PVLK3ux7EUOt+cZrHCdIFWbdOtxiPNIl1tkv9kV5aE5Aj2gJm4MeB9uXYhS\r\nOuksboBc0wyUGrl9+XZJ1+NlZOf7IjVi86CieK8= generated-by-azure\r\n\",\"path\":\"/home/azureuser/.ssh/authorized_keys\"}],\"publisher\":\"Canonical\",\"resourceGroupName\":\"sourabh-telemetry-sdk\",\"resourceId\":\"/subscriptions/8fba6d4f-7c37-4d13-9063-fd58ad2b86e2/resourceGroups/sourabh-telemetry-sdk/providers/Microsoft.Compute/virtualMachines/sourabh-testing\",\"securityProfile\":{\"secureBootEnabled\":\"false\",\"virtualTpmEnabled\":\"false\"},\"sku\":\"18.04-LTS\",\"storageProfile\":{\"dataDisks\":[],\"imageReference\":{\"id\":\"\",\"offer\":\"UbuntuServer\",\"publisher\":\"Canonical\",\"sku\":\"18.04-LTS\",\"version\":\"latest\"},\"osDisk\":{\"caching\":\"ReadWrite\",\"createOption\":\"FromImage\",\"diffDiskSettings\":{\"option\":\"\"},\"diskSizeGB\":\"30\",\"encryptionSettings\":{\"enabled\":\"false\"},\"image\":{\"uri\":\"\"},\"managedDisk\":{\"id\":\"/subscriptions/8fba6d4f-7c37-4d13-9063-fd58ad2b86e2/resourceGroups/sourabh-telemetry-sdk/providers/Microsoft.Compute/disks/sourabh-testing_OsDisk_1_9a54abfc5ba149c6a106bd9e5b558c2a\",\"storageAccountType\":\"Premium_LRS\"},\"name\":\"sourabh-testing_OsDisk_1_9a54abfc5ba149c6a106bd9e5b558c2a\",\"osType\":\"Linux\",\"vhd\":{\"uri\":\"\"},\"writeAcceleratorEnabled\":\"false\"}},\"subscriptionId\":\"8fba6d4f-7c37-4d13-9063-fd58ad2b86e2\",\"tags\":\"azsecpack:nonprod;platformsettings.host_environment.service.platform_optedin_for_rootcerts:true\",\"tagsList\":[{\"name\":\"azsecpack\",\"value\":\"nonprod\"},{\"name\":\"platformsettings.host_environment.service.platform_optedin_for_rootcerts\",\"value\":\"true\"}],\"version\":\"18.04.202103250\",\"vmId\":\"d0cb93eb-214b-4c2b-bd3d-cc93e90d9efd\",\"vmScaleSetName\":\"\",\"vmSize\":\"Standard_D2s_v3\",\"zone\":\"1\"},\"network\":{\"interface\":[{\"ipv4\":{\"ipAddress\":[{\"privateIpAddress\":\"10.0.7.5\",\"publicIpAddress\":\"\"}],\"subnet\":[{\"address\":\"10.0.7.0\",\"prefix\":\"24\"}]},\"ipv6\":{\"ipAddress\":[]},\"macAddress\":\"000D3A8F8BA0\"}]}}");
                        string payload = JsonConvert.SerializeObject(jsonObject);
                        result.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                        return Task.FromResult(result);
                    }
                    return null;
                }
            };

            List<string> preferredRegionList = new List<string>
            {
                "region1",
                "region2"
            };

            this.cosmosClientBuilder = TestCommon.GetDefaultConfiguration()
                                        .WithApplicationPreferredRegions(preferredRegionList)
                                        .WithTelemetryEnabled()
                                        .WithHttpClientFactory(() => new HttpClient(httpHandler));
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetrySchedulingInSeconds, null);
            Environment.SetEnvironmentVariable(ClientTelemetryOptions.EnvPropsClientTelemetryEndpoint, null);

            await base.TestCleanup();
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task PointSuccessOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);

            // Create an item
            ToDoActivity testItem = ToDoActivity.CreateRandomToDoActivity("MyTestPkValue");
            ItemResponse<ToDoActivity> createResponse = await container.CreateItemAsync<ToDoActivity>(testItem);
            ToDoActivity testItemCreated = createResponse.Resource;

            // Read an Item
            await container.ReadItemAsync<ToDoActivity>(testItem.id, new Cosmos.PartitionKey(testItem.id));

            // Upsert an Item
            await container.UpsertItemAsync<ToDoActivity>(testItem);

            // Replace an Item
            await container.ReplaceItemAsync<ToDoActivity>(testItemCreated, testItemCreated.id.ToString());

            // Patch an Item
            List<PatchOperation> patch = new List<PatchOperation>()
            {
                PatchOperation.Add("/new", "patched")
            };
            await ((ContainerInternal)container).PatchItemAsync<ToDoActivity>(
                testItem.id,
                new Cosmos.PartitionKey(testItem.id),
                patch);

            // Delete an Item
            await container.DeleteItemAsync<ToDoActivity>(testItem.id, new Cosmos.PartitionKey(testItem.id));

            await this.WaitAndAssert(12);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task PointReadFailureOperationsTest(ConnectionMode mode)
        {
            // Fail Read
            try
            {
                Container container = await this.GetContainer(mode);
                await container.ReadItemAsync<JObject>(
                    new Guid().ToString(),
                    new Cosmos.PartitionKey(new Guid().ToString()));
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                string message = ce.ToString();
                Assert.IsNotNull(message);
            }

            await this.WaitAndAssert(2);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task StreamReadFailureOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);

            // Fail Read
            try
            {
                await container.ReadItemStreamAsync(
                    new Guid().ToString(),
                    new Cosmos.PartitionKey(new Guid().ToString()));
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                string message = ce.ToString();
                Assert.IsNotNull(message);
            }

            await this.WaitAndAssert(2);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task StreamOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);
            // Create an item
            var testItem = new { id = "MyTestItemId", partitionKeyPath = "MyTestPkValue", details = "it's working", status = "done" };
            await container
                .CreateItemStreamAsync(TestCommon.SerializerCore.ToStream(testItem),
                new Cosmos.PartitionKey(testItem.id));

            //Upsert an Item
            await container.UpsertItemStreamAsync(TestCommon.SerializerCore.ToStream(testItem), new Cosmos.PartitionKey(testItem.id));

            //Read an Item
            await container.ReadItemStreamAsync(testItem.id, new Cosmos.PartitionKey(testItem.id));

            //Replace an Item
            await container.ReplaceItemStreamAsync(TestCommon.SerializerCore.ToStream(testItem), testItem.id, new Cosmos.PartitionKey(testItem.id));

            // Patch an Item
            List<PatchOperation> patch = new List<PatchOperation>()
            {
                PatchOperation.Add("/new", "patched")
            };
            await ((ContainerInternal)container).PatchItemStreamAsync(
                partitionKey: new Cosmos.PartitionKey(testItem.id),
                id: testItem.id,
                patchOperations: patch);

            //Delete an Item
            await container.DeleteItemStreamAsync(testItem.id, new Cosmos.PartitionKey(testItem.id));

            await this.WaitAndAssert(12);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task BatchOperationsTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);
            using (BatchAsyncContainerExecutor executor =
                new BatchAsyncContainerExecutor(
                    (ContainerInlineCore)container,
                    ((ContainerInlineCore)container).ClientContext,
                    20,
                    Documents.Constants.MaxDirectModeBatchRequestBodySizeInBytes)
                )
            {
                List<Task<TransactionalBatchOperationResult>> tasks = new List<Task<TransactionalBatchOperationResult>>();
                for (int i = 0; i < 10; i++)
                {
                    tasks.Add(executor.AddAsync(CreateItem(i.ToString()), NoOpTrace.Singleton, default));
                }

                await Task.WhenAll(tasks);
            }

            await this.WaitAndAssert(2);
        }

        [TestMethod]
        [DataRow(ConnectionMode.Direct)]
        [DataRow(ConnectionMode.Gateway)]
        public async Task QueryOperationTest(ConnectionMode mode)
        {
            Container container = await this.GetContainer(mode);

            ToDoActivity testItem = ToDoActivity.CreateRandomToDoActivity("MyTestPkValue", "MyTestItemId");
            ItemResponse<ToDoActivity> createResponse = await container.CreateItemAsync<ToDoActivity>(testItem);

            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                string sqlQueryText = "SELECT * FROM c";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<object> queryResultSetIterator = container.GetItemQueryIterator<object>(queryDefinition);

                List<object> families = new List<object>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<object> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (object family in currentResultSet)
                    {
                        families.Add(family);
                    }
                }
            }
            await this.WaitAndAssert(4);
        }

        private async Task WaitAndAssert(int expectedOperationCount)
        {
            Assert.IsNotNull(this.actualInfo, "Telemetry Information not available");

            // As this feature is thread based execution so wait for the results to avoid test flakiness
            List<ClientTelemetryProperties> localCopyOfActualInfo = null;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                lock (this.actualInfo)
                {
                    int operationCount = this.actualInfo.Sum(x => x.OperationInfo.Count);
                    Assert.IsTrue(operationCount <= expectedOperationCount, "More operations were recorded than expected");

                    if (operationCount == expectedOperationCount)
                    {
                        // Copy the list to avoid it being modified while validating
                        localCopyOfActualInfo = new List<ClientTelemetryProperties>(this.actualInfo);
                        break;
                    }

                    Assert.IsTrue(stopwatch.Elapsed.TotalMinutes < 1, $"The expected operation count was never hit.  ActualInfo:{JsonConvert.SerializeObject(this.actualInfo)}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
            while (localCopyOfActualInfo == null);

            List<OperationInfo> actualOperationList = new List<OperationInfo>();
            List<SystemInfo> actualSystemInformation = new List<SystemInfo>();

            // Asserting If basic client telemetry object is as expected
            foreach (ClientTelemetryProperties telemetryInfo in localCopyOfActualInfo)
            {
                actualOperationList.AddRange(telemetryInfo.OperationInfo);
                actualSystemInformation.AddRange(telemetryInfo.SystemInfo);

                // TODO: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2728
                //Assert.AreEqual(2, telemetryInfo.SystemInfo.Count, $"System Information Count doesn't Match; {JsonConvert.SerializeObject(telemetryInfo.SystemInfo)}");

                Assert.IsNotNull(telemetryInfo.GlobalDatabaseAccountName, "GlobalDatabaseAccountName is null");
                Assert.IsNotNull(telemetryInfo.DateTimeUtc, "Timestamp is null");
                Assert.AreEqual(2, telemetryInfo.PreferredRegions.Count);
                Assert.AreEqual("region1", telemetryInfo.PreferredRegions[0]);
                Assert.AreEqual("region2", telemetryInfo.PreferredRegions[1]);

            }
            Assert.AreEqual(expectedOperationCount, actualOperationList.Count, "Operation Information Count doesn't Match");

            // Asserting If operation list is as expected
            foreach (OperationInfo operation in actualOperationList)
            {
                Assert.IsNotNull(operation.Operation, "Operation Type is null");
                Assert.IsNotNull(operation.Resource, "Resource Type is null");
                Assert.IsNotNull(operation.ResponseSizeInBytes, "ResponseSizeInBytes is null");
                Assert.IsNotNull(operation.StatusCode, "StatusCode is null");
                Assert.IsNotNull(operation.Consistency, "Consistency is null");

                Assert.IsNotNull(operation.MetricInfo, "MetricInfo is null");
                Assert.IsNotNull(operation.MetricInfo.MetricsName, "MetricsName is null");
                Assert.IsNotNull(operation.MetricInfo.UnitName, "UnitName is null");
                Assert.IsNotNull(operation.MetricInfo.Percentiles, "Percentiles is null");
                Assert.IsTrue(operation.MetricInfo.Count > 0, "MetricInfo Count is not greater than 0");
                Assert.IsTrue(operation.MetricInfo.Mean >= 0, "MetricInfo Mean is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Max >= 0, "MetricInfo Max is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Min >= 0, "MetricInfo Min is not greater than or equal to 0");
            }

            // Asserting If system information list is as expected
            foreach (SystemInfo operation in actualSystemInformation)
            {
                Assert.IsNotNull(operation.MetricInfo, "MetricInfo is null");
                Assert.IsNotNull(operation.MetricInfo.MetricsName, "MetricsName is null");
                Assert.IsNotNull(operation.MetricInfo.UnitName, "UnitName is null");
                Assert.IsNotNull(operation.MetricInfo.Percentiles, "Percentiles is null");
                Assert.IsTrue(operation.MetricInfo.Count > 0, "MetricInfo Count is not greater than 0");
                Assert.IsTrue(operation.MetricInfo.Mean >= 0, "MetricInfo Mean is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Max >= 0, "MetricInfo Max is not greater than or equal to 0");
                Assert.IsTrue(operation.MetricInfo.Min >= 0, "MetricInfo Min is not greater than or equal to 0");
            }
        }

        private static ItemBatchOperation CreateItem(string itemId)
        {
            var testItem = new { id = itemId, Status = itemId };
            return new ItemBatchOperation(Documents.OperationType.Create, 0, new Cosmos.PartitionKey(itemId), itemId, TestCommon.SerializerCore.ToStream(testItem));
        }

        private async Task<Container> GetContainer(ConnectionMode mode)
        {
            if (mode == ConnectionMode.Direct)
            {
                this.cosmosClient = this.cosmosClientBuilder.WithConnectionModeDirect().Build();
            }

            if (mode == ConnectionMode.Gateway)
            {
                this.cosmosClient = this.cosmosClientBuilder.WithConnectionModeGateway().Build();
            }

            this.database = await this.cosmosClient.CreateDatabaseAsync(Guid.NewGuid().ToString());
            return await this.database.CreateContainerAsync(Guid.NewGuid().ToString(), "/id");
        }

    }
}
