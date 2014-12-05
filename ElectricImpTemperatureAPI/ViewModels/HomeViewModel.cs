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
        public TemperatureReading NestTemperatureReading { get; set; }
    }
}