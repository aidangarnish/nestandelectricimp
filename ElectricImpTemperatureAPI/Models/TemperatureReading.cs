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

        public DateTime UkTimeStamp 
        {
            get
            {
                TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                DateTime ukDateTime = TimeZoneInfo.ConvertTimeFromUtc(this.Timestamp.DateTime, ukTimeZone);
                return ukDateTime;
            }
        }

        public string UKTimeStampToDisplayString
        {
            get
            {
                return UkTimeStamp.ToString("dd/MM/yyyy HH:mm");
            }
        }
        public void Save()
        {
            temperatureReadingService.Save(this);
        }
    }
}