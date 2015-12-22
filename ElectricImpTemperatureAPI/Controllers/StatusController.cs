using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Models.DTOs;
using ElectricImpTemperatureAPI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace ElectricImpTemperatureAPI.Controllers
{
    public class StatusController : ApiController
    {
        public LatestTemperatures Get()
        {
             TemperatureReadingService temperatureReadingService = new TemperatureReadingService();
             IEnumerable<TemperatureReading> tempReadings = temperatureReadingService.TempByPartitionKey(DateTime.Today.ToString("dd-MM-yyyy"));

            LatestTemperatures latestTemperatures = new LatestTemperatures
            {
                MaplesRoom = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                SunRoom = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                Kitchen = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault()
            };
           
            return latestTemperatures;
        }
    }
}
