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

            HomeViewModel model = new HomeViewModel
            {
                MapleTemperatureReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["MaplesRoomDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                NestTemperatureReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["NestDeviceID"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                RaspberryPiTemperatureReading = tempReadings.Where(t => t.DeviceID == ConfigurationManager.AppSettings["RaspberryPi"]).OrderByDescending(t => t.Timestamp).FirstOrDefault(),
                HighestMapleTemperatureReading = temperatureReadingService.TempByPartitionKey(ConfigurationManager.AppSettings["MaplesRoomDeviceID"] + "-highest").FirstOrDefault(),
                LowestMapleTemperatureReading = temperatureReadingService.TempByPartitionKey(ConfigurationManager.AppSettings["MaplesRoomDeviceID"] + "-lowest").FirstOrDefault(),
                HighestNestTemperatureReading = temperatureReadingService.TempByPartitionKey(ConfigurationManager.AppSettings["NestDeviceID"] + "-highest").FirstOrDefault(),
                LowestNestTemperatureReading = temperatureReadingService.TempByPartitionKey(ConfigurationManager.AppSettings["NestDeviceID"] + "-lowest").FirstOrDefault(),
                HighestRaspberryPiTemperatureReading = temperatureReadingService.TempByPartitionKey(ConfigurationManager.AppSettings["RaspberryPi"] + "-highest").FirstOrDefault(),
                LowestRaspberryPiTemperatureReading = temperatureReadingService.TempByPartitionKey(ConfigurationManager.AppSettings["RaspberryPi"] + "-lowest").FirstOrDefault(),
            };
            return View(model);
        }

        public ActionResult Data() 
        {
            return View();
        }

    }
}