using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectricImpTemperatureAPI.Models.DTOs
{
    public class LatestTemperatures
    {
        public TemperatureReading MaplesRoom { get; set; }
        public TemperatureReading SunRoom { get; set; }
        public TemperatureReading Kitchen { get; set; }
    }
}