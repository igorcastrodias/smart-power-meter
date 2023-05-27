using Microsoft.AspNetCore.Mvc;
using SmartPowerMeter.Application;

namespace SmartPowerMeter.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EnergyMeasurement : ControllerBase
{
    private readonly ILogger<EnergyMeasurement> _logger;
    private readonly IEnergyMeasurementRepository _repository;

    public EnergyMeasurement(ILogger<EnergyMeasurement> logger, IEnergyMeasurementRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet(Name = "EnergyMeasurementGet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IEnumerable<EnergyMeasurementDailyTotal>> Get()
    {
        var measurements = await _repository.GetDailyTotalMeasurementsLast30Days();
        return measurements;

    }

    [HttpPost(Name = "EnergyMeasurementPost")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Post(Domain.Dtos.EnergyMeasurement energyMeasurement)
    {
        await _repository.AddMeasurementAsync(energyMeasurement);

        return new OkObjectResult("Salvo no banco de dados");

    }

}
