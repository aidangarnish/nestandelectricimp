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

            //check if it is night time and if the temp is below 15.5 degreees C
            //If it is then kick heating on using the NEST API
            if ((DateTime.UtcNow.Hour >= 22 || DateTime.UtcNow.Hour <= 7) )
            {
                double targetTemp = 0.0;
                if (currentReadingInMaplesRoom.Temperature < 15.5)
                {
                    //turn on nest by setting the target temp to be 1.0 degree higher than the ambient temp
                    targetTemp = nestThermostat.ambient_temperature_c + 1.0;
                    UpdateTargetTemperature(targetTemp);
                    nestTempReading.AdditionalInformation = "Nest thermostat target temperature increased to " + targetTemp;
                }
                else if (currentReadingInMaplesRoom.Temperature > 18.0)
                {   
                    //turn Nest off if temp in Maples room is higher than 18 degrees by setting Nest target temp to be the same as the ambient temp
                    targetTemp = nestThermostat.ambient_temperature_c;
                    UpdateTargetTemperature(targetTemp);
                    nestTempReading.AdditionalInformation = "Nest thermostat target temperature set to " + targetTemp; 
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
