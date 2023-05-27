using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartPowerMeter.Application;

namespace SmartPowerMeter
{
    public class EnergyMeasurementGet
    {
        private readonly IEnergyMeasurementRepository _repository;

        public EnergyMeasurementGet(IEnergyMeasurementRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("EnergyMeasurementGet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var measurements = await _repository.GetDailyTotalMeasurementsLast30Days();
            return new OkObjectResult(measurements);

        }
    }
}

