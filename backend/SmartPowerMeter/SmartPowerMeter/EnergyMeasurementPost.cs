using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartPowerMeter.Application;
using SmartPowerMeter.Domain;
using SmartPowerMeter.Domain.Dtos;

namespace SmartPowerMeter
{
    public class EnergyMeasurementPost
    {

        private readonly IEnergyMeasurementRepository _repository;

        public EnergyMeasurementPost(IEnergyMeasurementRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("EnergyMeasurementPost")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var measurement = JsonConvert.DeserializeObject<EnergyMeasurement>(requestBody);

                await _repository.AddMeasurementAsync(measurement);

                return new OkObjectResult("Salvo no banco de dados");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred.");
                return new OkObjectResult("Erro no banco de dados");
            }
        }
    }
}

