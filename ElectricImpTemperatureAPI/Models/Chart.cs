using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectricImpTemperatureAPI.Models
{
    public class Chart
    {
        public List<string> labels { get; set; }
        public List<Dataset> datasets { get; set; }
    }

    public class Dataset
    {
        public string label { get; set; }
        public string fillColor { get; set; }
        public string strokeColor { get; set; }
        public string pointColor { get; set; }
        public string pointStrokeColor { get; set; }
        public string pointHighlightFill { get; set; }
        public string pointHighlightStroke { get; set; }
        public List<double> data { get; set; }
    }

}