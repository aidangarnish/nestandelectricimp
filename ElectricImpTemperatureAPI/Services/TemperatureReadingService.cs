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
            Save(tempReading, DateTime.Today.ToString("dd-MM-yyyy"));

            SaveRecordReadings(tempReading);  
        }

        private void Save(TemperatureReading tempReading, string partitionKey)
        {
            tempReading.PartitionKey = partitionKey;

            CloudStorageAccount account = CloudStorageAccount.Parse(ATSConnectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(TableName);
            table.CreateIfNotExists();

            TableOperation insertOperation = TableOperation.InsertOrMerge(tempReading);
            table.Execute(insertOperation);
        }

        private void SaveRecordReadings(TemperatureReading tempReading)
        {
            //partition keys for highest readings
            //highest ever for device = "[deviceId]-highest"
            //lowest ever for device = "[deviceId]-lowest"
            //highest for device this month = "[deviceId]-[MM-yyyy]-highest"
            //lowest for device this month = "[deviceId]-[MM-yyyy]-lowest"
            //highest for device today = "[deviceId]-[dd-MM-yyyy]-highest"
            //lowest for device today = "[deviceId]-[dd-MM-yyyy]-lowest"

            //get/set highest ever reading for device
            string highestReadingPartitionKey = tempReading.DeviceID + "-highest";
            var highestTempForDevice = TempByPartitionKey(highestReadingPartitionKey).FirstOrDefault();
            if(highestTempForDevice == null || highestTempForDevice.Temperature < tempReading.Temperature)
            {
                tempReading.RowKey = highestTempForDevice != null ? highestTempForDevice.RowKey : tempReading.RowKey;
                Save(tempReading, highestReadingPartitionKey);
            }

            //get/set lowest ever reading for device
            string lowestReadingPartitionKey = tempReading.DeviceID + "-lowest";
            var lowestTempForDevice = TempByPartitionKey(lowestReadingPartitionKey).FirstOrDefault();
            if (highestTempForDevice == null || lowestTempForDevice.Temperature > tempReading.Temperature)
            {
                tempReading.RowKey = lowestTempForDevice != null ? lowestTempForDevice.RowKey : tempReading.RowKey;
                Save(tempReading, lowestReadingPartitionKey);
            }


            //get/set highest reading for device this month
            string highestReadingThisMonthPartitionKey = tempReading.DeviceID + DateTime.UtcNow.Month + "-" + DateTime.UtcNow.Year + "-highest";
            var highestTempForDeviceThisMonth = TempByPartitionKey(highestReadingThisMonthPartitionKey).FirstOrDefault();
            if (highestTempForDeviceThisMonth == null || highestTempForDeviceThisMonth.Temperature < tempReading.Temperature)
            {
                tempReading.RowKey = highestTempForDeviceThisMonth != null ? highestTempForDeviceThisMonth.RowKey : tempReading.RowKey;
                Save(tempReading, highestReadingThisMonthPartitionKey);
            }

            //get/set lowest reading for device this month
            string lowestReadingThisMonthPartitionKey = tempReading.DeviceID + DateTime.UtcNow.Month + "-" + DateTime.UtcNow.Year + "-lowest";
            var lowestTempForDeviceThisMonth = TempByPartitionKey(lowestReadingThisMonthPartitionKey).FirstOrDefault();
            if (lowestTempForDeviceThisMonth == null || lowestTempForDeviceThisMonth.Temperature > tempReading.Temperature)
            {
                tempReading.RowKey = lowestTempForDeviceThisMonth != null ? lowestTempForDeviceThisMonth.RowKey : tempReading.RowKey;
                Save(tempReading, lowestReadingThisMonthPartitionKey);
            }
        }
    }
}