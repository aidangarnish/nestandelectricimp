using ElectricImpTemperatureAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ElectricImpTemperatureAPI.Controllers
{
    public class TemperatureController : ApiController
    {
        public HttpResponseMessage Post([FromBody]TemperatureReading model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "value");

            try
            {
                model.Save();
            }
            catch
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
    }
}