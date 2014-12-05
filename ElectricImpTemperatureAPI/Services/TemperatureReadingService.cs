using ElectricImpTemperatureAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ElectricImpTemperatureAPI.Services
{
    public class TemperatureReadingService
    {
        private string ATSConnectionString;
        private string TableName;
        private string LastTracksTableName;
        private CloudStorageAccount cloudStorageAccount;
     
        public TemperatureReadingService()
        {
            this.ATSConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            TableName = "TemperatureLogs";
            cloudStorageAccount = CloudStorageAccount.Parse(ATSConnectionString);
        }

        public IEnumerable<TemperatureReading> TempByPartitionKey(string partitionKey)
        {
            try
            {
                // Create the table client.
                CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference(TableName);

                TableQuery<TemperatureReading> query = new TableQuery<TemperatureReading>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                IEnumerable<TemperatureReading> tempReadings = table.ExecuteQuery(query);

                return tempReadings;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        public IEnumerable<TemperatureReading> TempByPartitionKeyAndDeviceIdentifier(string partitionKey, string deviceID)
        {
            // Create the table client.
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(TableName);

            TableQuery<TemperatureReading> query = new TableQuery<TemperatureReading>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("DeviceID", QueryComparisons.Equal, deviceID)));

            IEnumerable<TemperatureReading> tempReadings = table.ExecuteQuery(query);

            return tempReadings;
        }

        public void Save(TemperatureReading tempReading)
        {
            tempReading.PartitionKey = DateTime.Today.ToString("dd-MM-yyyy");

            CloudStorageAccount account = CloudStorageAccount.Parse(ATSConnectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(TableName);
            table.CreateIfNotExists();

            TableOperation insertOperation = TableOperation.Insert(tempReading);
            table.Execute(insertOperation);
        }
    }
}