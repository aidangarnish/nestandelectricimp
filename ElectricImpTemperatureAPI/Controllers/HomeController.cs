using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Services;
using ElectricImpTemperatureAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectricImpTemperatureAPI.Controllers
{
    public class HomeController : Controller
    {
        private TemperatureReadingService temperatureReadingService;

        public HomeController()
        {
            temperatureReadingService = new TemperatureReadingService();
        }
        // GET: Home
        public ActionResult Index()
        {
            IEnumerable<TemperatureReading> tempReadings = temperatureReadingService.TempByPartitionKey(DateTime.Today.ToString("dd-MM-yyyy"));

            IEnumerable<TemperatureReading> highestTempReadings = temperatureReadingService.TempByPartitionKey("highest");
            IEnumerable<TemperatureReading> lowestTempReadings = temperatureReadingService.TempByPartitionKey("lowest");

            IEnumerable<TemperatureReading> highestTempReadingsToday = temperatureReadingService.TempByPartitionKey(DateTime.UtcNow.ToString("dd-MM-yyyy") + "-highest");
            IEnumerable<TemperatureReading> lowestTempReadingsToday = temperatureReadingService.TempByPartitionKey(DateTime.UtcNow.ToString("dd-MM-yyyy") + "-lowest");

            HomeViewModel model = new HomeViewModel
            {
                MapleTemperatureReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                NestTemperatureReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                RaspberryPiTemperatureReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),

                HighestMapleTemperatureReading = highestTempReadings.Where(d => d.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).FirstOrDefault(),
                LowestMapleTemperatureReading = lowestTempReadings.Where(d => d.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).FirstOrDefault(),
                HighestNestTemperatureReading = highestTempReadings.Where(d => d.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).FirstOrDefault(),
                LowestNestTemperatureReading = lowestTempReadings.Where(d => d.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).FirstOrDefault(),
                HighestRaspberryPiTemperatureReading = highestTempReadings.Where(d => d.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).FirstOrDefault(),
                LowestRaspberryPiTemperatureReading = lowestTempReadings.Where(d => d.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).FirstOrDefault(),

                HighestMapleTemperatureReadingToday = highestTempReadingsToday.Where(d => d.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).FirstOrDefault(),
                LowestMapleTemperatureReadingToday = lowestTempReadingsToday.Where(d => d.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).FirstOrDefault(),
                HighestNestTemperatureReadingToday = highestTempReadingsToday.Where(d => d.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).FirstOrDefault(),
                LowestNestTemperatureReadingToday = lowestTempReadingsToday.Where(d => d.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).FirstOrDefault(),
                HighestRaspberryPiTemperatureReadingToday = highestTempReadingsToday.Where(d => d.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).FirstOrDefault(),
                LowestRaspberryPiTemperatureReadingToday = lowestTempReadingsToday.Where(d => d.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).FirstOrDefault(),

            };
            return View(model);
        }

        public ActionResult Data() 
        {
            return View();
        }

    }
}