using ElectricImpTemperatureAPI.Models;
using ElectricImpTemperatureAPI.Services;
using Keen.Core;
using NestWebJob;
using Newtonsoft.Json;
using RestackIO.Net;
using RestackIO.Net.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public HttpResponseMessage Post([FromBody]TemperatureReading temperatureReading)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "value");

            try
            {
                temperatureReading.Save();

                if (temperatureReading.DeviceID != "RaspberryPi")
                {
                    NestThermostat nestThermostat = GetCurrentNestValues();

                    TemperatureReading nestTempReading = new TemperatureReading
                    {
                        Temperature = nestThermostat.ambient_temperature_c,
                        DeviceID = NestDeviceID
                    };

                    CheckBedroomTemperature(temperatureReading, nestTempReading, nestThermostat);

                    SaveDataToKeenIO(nestTempReading);
                    

                    SaveDataToRestackIO(nestTempReading, temperatureReading);
                }

                SaveDataToKeenIO(temperatureReading);
            }
            catch
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
       
        private void CheckBedroomTemperature(TemperatureReading bedroomReading, TemperatureReading nestTempReading, NestThermostat nestThermostat)
        {
            TemperatureReadingService tempReadingService = new TemperatureReadingService();

            //check if it is night time and if the temp is below target
            //If it is then kick heating on using the NEST API

            if ((DateTime.UtcNow.Hour >= 22 || DateTime.UtcNow.Hour <= 6) && ConfigurationManager.AppSettings["Active"] == "true")
            {
                double nestTargetTemp = 0.0;

                if (bedroomReading.Temperature < TargetTemp - 0.2)
                {
                    nestTargetTemp = nestThermostat.ambient_temperature_c + 1.0;
                    if (nestTargetTemp != nestThermostat.target_temperature_c)
                    {
                        UpdateTargetTemperature(nestTargetTemp);
                        nestTempReading.AdditionalInformation = "Nest thermostat target temperature increased to " + nestTargetTemp;
                    }
                }
                else if (bedroomReading.Temperature > TargetTemp + 0.2)
                {
                    //turn Nest off if temp in Maples room is at acceptable temp
                    nestTargetTemp = nestThermostat.ambient_temperature_c - 0.5;
                    if (nestTargetTemp != nestThermostat.target_temperature_c)
                    {
                        UpdateTargetTemperature(nestTargetTemp);
                        nestTempReading.AdditionalInformation = "Nest thermostat target temperature reduced to " + nestTargetTemp;
                    }
                }
            }

            tempReadingService.Save(nestTempReading);
        }

        private void SaveDataToKeenIO(TemperatureReading tempReading)
        {
            var prjSettings = new ProjectSettingsProvider("5492ab1396773d1189271310", writeKey: "9da5be2490ad287a8d2ba7c1d874107a6f17f7fcc11addcb5bc46ac29ad380b301d99ef5817e50cbbad82a6745a1db5dc7d90493c84e32c60d15119eb2efb96745625e0a5c583d4dda5eae342b94fe9f35efe39085634e47a6b6c7b894b07e516f258b650a9d453095a353b055a98ca9");
            var keenClient = new KeenClient(prjSettings);
            
            keenClient.AddEvent("temperaturereadings", tempReading);
        }

        private void SaveDataToRestackIO(TemperatureReading nestTemp, TemperatureReading bedroomTemp)
        {
            Restack restack = new Restack("876d6d51c7694bdb8f007737e55037e0");

            restack.SaveData("3784afddc22949c9990acdde6db92099", "Temperature", new Value() { value = new decimal(nestTemp.Temperature) });

            restack.SaveData("96bffc1b1bce4dfb87ca6f8e80b28a33", "Temperature", new Value { value = new decimal(bedroomTemp.Temperature) });       
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