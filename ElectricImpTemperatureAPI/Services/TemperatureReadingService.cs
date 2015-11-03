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

            tempReading.RowKey = tempReading.DeviceID;

            //get/set highest ever reading for device
            string highestReadingPartitionKey = "highest";
            var highestTempForDevice = TempByPartitionKeyAndDeviceIdentifier(highestReadingPartitionKey, tempReading.DeviceID).FirstOrDefault();
            if(highestTempForDevice == null || highestTempForDevice.Temperature < tempReading.Temperature)
            {
                tempReading.RowKey = highestTempForDevice != null ? highestTempForDevice.RowKey : tempReading.RowKey;
                Save(tempReading, highestReadingPartitionKey);
            }

            //get/set lowest ever reading for device
            string lowestReadingPartitionKey = "lowest";
            var lowestTempForDevice = TempByPartitionKeyAndDeviceIdentifier(lowestReadingPartitionKey, tempReading.DeviceID).FirstOrDefault();
            if (highestTempForDevice == null || lowestTempForDevice.Temperature > tempReading.Temperature)
            {
                Save(tempReading, lowestReadingPartitionKey);
            }


            //get/set highest reading for device this month
            string highestReadingThisMonthPartitionKey = DateTime.UtcNow.ToString("MM-yyyy") + "-highest";
            var highestTempForDeviceThisMonth = TempByPartitionKeyAndDeviceIdentifier(highestReadingThisMonthPartitionKey, tempReading.DeviceID).FirstOrDefault();
            if (highestTempForDeviceThisMonth == null || highestTempForDeviceThisMonth.Temperature < tempReading.Temperature)
            {
                Save(tempReading, highestReadingThisMonthPartitionKey);
            }

            //get/set lowest reading for device this month
            string lowestReadingThisMonthPartitionKey = DateTime.UtcNow.ToString("MM-yyyy") + "-lowest";
            var lowestTempForDeviceThisMonth = TempByPartitionKeyAndDeviceIdentifier(lowestReadingThisMonthPartitionKey, tempReading.DeviceID).FirstOrDefault();
            if (lowestTempForDeviceThisMonth == null || lowestTempForDeviceThisMonth.Temperature > tempReading.Temperature)
            {
                Save(tempReading, lowestReadingThisMonthPartitionKey);
            }


            //get/set highest reading for device today
            string highestReadingTodayPartitionKey = DateTime.UtcNow.ToString("dd-MM-yyyy") + "-highest";
            var highestTempForDeviceToday = TempByPartitionKeyAndDeviceIdentifier(highestReadingTodayPartitionKey, tempReading.DeviceID).FirstOrDefault();
            if (highestTempForDeviceToday == null || highestTempForDeviceToday.Temperature < tempReading.Temperature)
            {
                Save(tempReading, highestReadingTodayPartitionKey);
            }

            //get/set lowest reading for device today
            string lowestReadingTodayPartitionKey = DateTime.UtcNow.ToString("dd-MM-yyyy") + "-lowest";
            var lowestTempForDeviceToday = TempByPartitionKeyAndDeviceIdentifier(lowestReadingTodayPartitionKey, tempReading.DeviceID).FirstOrDefault();
            if (lowestTempForDeviceToday == null || lowestTempForDeviceToday.Temperature > tempReading.Temperature)
            {
                tempReading.RowKey = lowestTempForDeviceToday != null ? lowestTempForDeviceToday.RowKey : tempReading.RowKey;
                Save(tempReading, lowestReadingTodayPartitionKey);
            }
        }
    }
}