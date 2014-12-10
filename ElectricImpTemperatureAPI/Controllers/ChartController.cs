using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectricImpTemperatureAPI.Controllers
{
    public class ChartController : Controller
    {
        private TemperatureReadingService temperatureReadingService;
        private string MapleRoomDeviceID;
        private string NestDeviceID;
       


        public ChartController()
        {
            temperatureReadingService = new TemperatureReadingService();
            MapleRoomDeviceID = ConfigurationManager.AppSettings["MaplesRoomDeviceID"];
            NestDeviceID = ConfigurationManager.AppSettings["NestDeviceID"];
           

        }
        // GET: Chart
        public JsonResult Last3Hours()
        {
            Chart chart = GetChart(180, 5);
 
            return Json(chart, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Today()
        {
            int minutesToday = DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute;
            int remainder = minutesToday % 15;
            int minutesToChart = minutesToday - remainder;

            Chart chart = GetChart(minutesToChart, 15);
            
            return Json(chart, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Rickshaw()
        {
            IEnumerable<TemperatureReading> maplesReadings = temperatureReadingService.TempByPartitionKeyAndDeviceIdentifier(DateTime.UtcNow.ToString("dd-MM-yyyy"), MapleRoomDeviceID);
            IEnumerable<TemperatureReading> nestReadings = temperatureReadingService.TempByPartitionKeyAndDeviceIdentifier(DateTime.UtcNow.ToString("dd-MM-yyyy"), NestDeviceID);


            RickshawDataSeries dataSeries = new RickshawDataSeries
            {
                series = new List<Series>()
            };

            Series snugSeries = new Series
            {
                data = new List<Datum>(),
                color= "steelblue",
			    name= "Snug"
                 
            };

            foreach(TemperatureReading tempReading in nestReadings.OrderBy(t => t.Timestamp))
            {
                long unixTime = ToUnixTime(tempReading.Timestamp.DateTime);
                Datum datum = new Datum
                {
                    x = unixTime,
                    y = tempReading.Temperature
                };

                snugSeries.data.Add(datum);
            }

            Series mapleSeries = new Series
            {
                data = new List<Datum>(),
                color = "lightblue",
                name = "Maple"

            };

            foreach(TemperatureReading tempReading in maplesReadings.OrderBy(t => t.Timestamp))
            {
                long unixTime =  ToUnixTime(tempReading.Timestamp.DateTime);
                Datum datum = new Datum
                {
                    x = unixTime,
                    y = tempReading.Temperature
                };

                mapleSeries.data.Add(datum);

            }

            dataSeries.series.Add(snugSeries);
            dataSeries.series.Add(mapleSeries);

      
            return Json(dataSeries, JsonRequestBehavior.AllowGet);
        }

        private Chart GetChart(int duration, int interval)
        {
            List<DateTime> labelDateTimes = new List<DateTime>();
            List<string> labels = GetLabels(duration, interval, labelDateTimes);

            DateTime firstLabelDateTime = labelDateTimes.OrderBy(t => t.TimeOfDay).FirstOrDefault();
            
            Dataset maplesRoomDataset = new Dataset
            {
                label = "Maple's room",
                fillColor = "rgba(151,187,205,0.2)",
                strokeColor = "rgba(151,187,205,1)",
                pointColor = "rgba(151,187,205,1)",
                pointStrokeColor = "#fff",
                pointHighlightFill = "#fff",
                pointHighlightStroke = "rgba(151,187,205,1)"
            };

            IEnumerable<TemperatureReading> maplesReadings = temperatureReadingService.TempByPartitionKeyAndDeviceIdentifier(DateTime.UtcNow.ToString("dd-MM-yyyy"), MapleRoomDeviceID).Where(r => r.Timestamp >= firstLabelDateTime);

            maplesRoomDataset.data = BuildDataSet(labelDateTimes, maplesReadings);

            Dataset nestDataset = new Dataset
            {
                label = "Nest - snug",
                fillColor = "rgba(220,220,220,0.2)",
                strokeColor = "rgba(220,220,220,1)",
                pointColor = "rgba(220,220,220,1)",
                pointStrokeColor = "#fff",
                pointHighlightFill = "#fff",
                pointHighlightStroke = "rgba(220,220,220,1)"
            };

            

            IEnumerable<TemperatureReading> nestReadings = temperatureReadingService.TempByPartitionKeyAndDeviceIdentifier(DateTime.UtcNow.ToString("dd-MM-yyyy"), NestDeviceID).Where(r => r.Timestamp >= firstLabelDateTime);

            nestDataset.data = BuildDataSet(labelDateTimes, nestReadings);

            Chart chart = new Chart
            {
                labels = labels,
                datasets = new List<Dataset> { nestDataset, maplesRoomDataset }
            };

            return chart;
        }
        private List<double> BuildDataSet(List<DateTime> labelDateTimes, IEnumerable<TemperatureReading> temperatureReadings)
        {

            List<double> data = new List<double>();

            foreach (DateTime labelDateTime in labelDateTimes)
            {

                double minDiff = 999999.0;
                TemperatureReading closestReading = null;
                foreach (TemperatureReading reading in temperatureReadings)
                {
                    double diffInSeconds = Math.Abs((labelDateTime - reading.Timestamp).TotalSeconds);
                    if (diffInSeconds < minDiff)
                    {
                        closestReading = reading;
                        minDiff = diffInSeconds;
                        if (minDiff < 120) { break; }
                    }
                }

                if (closestReading != null)
                {
                    data.Add(closestReading.Temperature);
                }
 
            }
           
            return data;
        }

        private List<string> GetLabels(int minutes, int interval, List<DateTime> labelDateTimes)
        {
            DateTime currentDateTime = DateTime.UtcNow;
            int currentMinute = currentDateTime.Minute;
            int currentMinuteRemainder = currentMinute % interval;
            DateTime lastLabelDateTime = currentDateTime.AddMinutes(-currentMinuteRemainder);

            List<string> labels = new List<string>();
            for (int i = minutes/interval; i >= 0; i--)
            {
                DateTime labelDateTime = lastLabelDateTime.AddMinutes(-(i * interval));
                labels.Add(labelDateTime.ToString("HH:mm"));
                labelDateTimes.Add(labelDateTime);
            }

            return labels;
        }

        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        private long ToUnixTime(DateTime dateTime)
        {
            DateTime dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return Convert.ToInt64((dt - epoch).TotalSeconds);
        }

    }
}