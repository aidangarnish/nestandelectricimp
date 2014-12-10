using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Services;
using NestWebJob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ElectricImpTemperatureAPI.Controllers
{
    public class TemperatureController : ApiController
    {
        private string NestThermostatUrl;
        private string NestThermostatID;
        private string NestAuthToken;
        private double TargetTemp;
        private string NestDeviceID;

        public TemperatureController()
        {
            NestThermostatUrl = ConfigurationManager.AppSettings["NestThermostatUrl"];
            NestThermostatID = ConfigurationManager.AppSettings["NestThermostatID"];
            NestAuthToken = ConfigurationManager.AppSettings["NestAuthToken"];
            TargetTemp = Convert.ToDouble(ConfigurationManager.AppSettings["TargetTemp"]);
            NestDeviceID = ConfigurationManager.AppSettings["NestDeviceID"];
        }
        public HttpResponseMessage Post([FromBody]TemperatureReading model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "value");

            try
            {
                model.Save();

                CheckNest();
            }
            catch
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        private void CheckNest()
        {
            TemperatureReadingService tempReadingService = new TemperatureReadingService();

            IEnumerable<TemperatureReading> tempReadings = tempReadingService.TempByPartitionKeyAndDeviceIdentifier(DateTime.Today.ToString("dd-MM-yyyy"), "2338b2ab5ba5ceee");

            TemperatureReading currentReadingInMaplesRoom = tempReadings.OrderByDescending(t => t.Timestamp).FirstOrDefault();

            NestThermostat nestThermostat = GetCurrentNestValues();

            //store nest value
            TemperatureReading nestTempReading = new TemperatureReading
            {
                Temperature = nestThermostat.ambient_temperature_c,
                DeviceID = NestDeviceID
            };

            //check if it is night time and if the temp is below target
            //If it is then kick heating on using the NEST API#

            if ((DateTime.UtcNow.Hour >= 22 || DateTime.UtcNow.Hour <= 7))
            {
                double nestTargetTemp = 0.0;

                if (currentReadingInMaplesRoom.Temperature < TargetTemp - 0.2)
                {
                    nestTargetTemp = nestThermostat.ambient_temperature_c + 1.0;
                    UpdateTargetTemperature(nestTargetTemp);
                    nestTempReading.AdditionalInformation = "Nest thermostat target temperature increased to " + nestTargetTemp;
                }
                else if (currentReadingInMaplesRoom.Temperature > TargetTemp + 0.2)
                {
                    //turn Nest off if temp in Maples room is at acceptable temp
                    nestTargetTemp = nestThermostat.ambient_temperature_c - 3.0;
                    UpdateTargetTemperature(nestTargetTemp);
                    nestTempReading.AdditionalInformation = "Nest thermostat target temperature reduced to " + nestTargetTemp;
                }
            }

            tempReadingService.Save(nestTempReading);
        }

        private NestThermostat GetCurrentNestValues()
        {
            WebClient client = new WebClient();
            string url = NestThermostatUrl + NestThermostatID + "?auth=" + NestAuthToken;

            string response = client.DownloadString(url);

            return JsonConvert.DeserializeObject<NestThermostat>(response);

        }
        private void UpdateTargetTemperature(double targetTemp)
        {
            WebClient client = new WebClient();
            string url = NestThermostatUrl + NestThermostatID + "?auth=" + NestAuthToken;

            string postData = "{\"target_temperature_c\": " + targetTemp + "}";
            byte[] postArray = Encoding.ASCII.GetBytes(postData);
            client.Headers.Add("Content-Type", "application/json");

            byte[] responseBytes = client.UploadData(url, "PUT", postArray);
        }
    }
}