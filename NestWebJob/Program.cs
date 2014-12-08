using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NestWebJob
{
    class Program
    {
        private string NestThermostatUrl = ConfigurationManager.AppSettings["NestThermostatUrl"];
        private string NestThermostatID = ConfigurationManager.AppSettings["NestThermostatID"];
        private string NestAuthToken = ConfigurationManager.AppSettings["NestAuthToken"];
        private double TargetTemp = Convert.ToDouble(ConfigurationManager.AppSettings["TargetTemp"]);

        static void Main(string[] args)
        {
            Program program = new Program();
            program.CheckNest();
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
                DeviceID = "Nest"
            };

            //check if it is night time and if the temp is below target
            //If it is then kick heating on using the NEST API#

            if ((DateTime.UtcNow.Hour >= 22 || DateTime.UtcNow.Hour <= 7) )
            {
                double nestTargetTemp = 0.0;

                if (currentReadingInMaplesRoom.Temperature <  TargetTemp - 0.5)
                {
                    nestTargetTemp = nestThermostat.ambient_temperature_c + 1.0;
                    UpdateTargetTemperature(nestTargetTemp);
                    nestTempReading.AdditionalInformation = "Nest thermostat target temperature increased to " + nestTargetTemp;   
                }
                else if (currentReadingInMaplesRoom.Temperature > TargetTemp + 0.5)
                {   
                    //turn Nest off if temp in Maples room is at acceptable temp
                    nestTargetTemp = nestThermostat.ambient_temperature_c - 3.0;
                    UpdateTargetTemperature(nestTargetTemp);
                    nestTempReading.AdditionalInformation = "Nest thermostat target temperature set to " + nestTargetTemp; 
                }
            }

            tempReadingService.Save(nestTempReading);
        }

        private NestThermostat GetCurrentNestValues()
        {
            WebClient client = new WebClient();
            string url = NestThermostatUrl + NestThermostatID + "?auth="+NestAuthToken;

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
