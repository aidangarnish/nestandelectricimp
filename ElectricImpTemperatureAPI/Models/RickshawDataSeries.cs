using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectricImpTemperatureAPI.Models
{
    public class RickshawDataSeries    
    {        
        public List<Series> series { get; set; }
    }
    public class Datum
    {
        public long x { get; set; }
        public double y { get; set; }
    }
    public class Series
    {
        public List<Datum> data { get; set; }
        public string color { get; set; }
        public string name { get; set; }
    }

    
}