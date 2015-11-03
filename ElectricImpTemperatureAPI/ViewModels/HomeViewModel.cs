using ElectricImpTemperatureAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectricImpTemperatureAPI.ViewModels
{
    public class HomeViewModel
    {
        public TemperatureReading MapleTemperatureReading { get; set; }
        public TemperatureReading HighestMapleTemperatureReading { get; set; }
        public TemperatureReading LowestMapleTemperatureReading { get; set; }
        public TemperatureReading NestTemperatureReading { get; set; }
        public TemperatureReading HighestNestTemperatureReading { get; set; }
        public TemperatureReading LowestNestTemperatureReading { get; set; }
        public TemperatureReading RaspberryPiTemperatureReading { get; set; }
        public TemperatureReading HighestRaspberryPiTemperatureReading { get; set; }
        public TemperatureReading LowestRaspberryPiTemperatureReading { get; set; }
    }
}