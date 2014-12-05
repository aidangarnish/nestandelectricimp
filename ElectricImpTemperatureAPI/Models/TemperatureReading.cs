using ElectricImpTemperatureAPI.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ElectricImpTemperatureAPI.Models
{
    public class TemperatureReading : TableEntity
    {
        private TemperatureReadingService temperatureReadingService;

        public TemperatureReading()
        {
            RowKey = Guid.NewGuid().ToString();
            temperatureReadingService = new TemperatureReadingService();
        }

        public string DeviceID { get; set; }
        public double Temperature { get; set; }
        public string AdditionalInformation { get; set; }

        public void Save()
        {
            temperatureReadingService.Save(this);
        }
    }
}