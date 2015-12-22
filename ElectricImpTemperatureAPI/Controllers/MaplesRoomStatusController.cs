using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElectricImpTemperatureAPI.Controllers
{
    public class MaplesRoomStatusController : ApiController
    {
        public TemperatureReading Get()
        {
            TemperatureReadingService temperatureReadingService = new TemperatureReadingService();
            IEnumerable<TemperatureReading> tempReadings = temperatureReadingService.TempByPartitionKey(DateTime.Today.ToString("dd-MM-yyyy"));


            var latestMaplesRoomTempReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault();
            latestMaplesRoomTempReading.DeviceID = "Maples Room";

            return latestMaplesRoomTempReading;
        }
    }
}
