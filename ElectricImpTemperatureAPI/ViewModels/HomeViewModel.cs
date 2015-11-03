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
        public TemperatureReading HighestMapleTemperatureReadingThisMonth { get; set; }
        public TemperatureReading LowestMapleTemperatureReadingThisMonth { get; set; }
        public TemperatureReading HighestMapleTemperatureReadingToday { get; set; }
        public TemperatureReading LowestMapleTemperatureReadingToday { get; set; }
        public TemperatureReading NestTemperatureReading { get; set; }
        public TemperatureReading HighestNestTemperatureReading { get; set; }
        public TemperatureReading LowestNestTemperatureReading { get; set; }
        public TemperatureReading HighestNestTemperatureReadingThisMonth { get; set; }
        public TemperatureReading LowestNestTemperatureReadingThisMonth { get; set; }
        public TemperatureReading HighestNestTemperatureReadingToday { get; set; }
        public TemperatureReading LowestNestTemperatureReadingToday { get; set; }
        public TemperatureReading RaspberryPiTemperatureReading { get; set; }
        public TemperatureReading HighestRaspberryPiTemperatureReading { get; set; }
        public TemperatureReading LowestRaspberryPiTemperatureReading { get; set; }
        public TemperatureReading HighestRaspberryPiTemperatureReadingThisMonth { get; set; }
        public TemperatureReading LowestRaspberryPiTemperatureReadingThisMonth { get; set; }
        public TemperatureReading HighestRaspberryPiTemperatureReadingToday { get; set; }
        public TemperatureReading LowestRaspberryPiTemperatureReadingToday { get; set; }
    }
}