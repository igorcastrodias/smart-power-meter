using System.Collections.Generic;
using System.Threading.Tasks;
using SmartPowerMeter.Domain;
using SmartPowerMeter.Domain.Dtos;

namespace SmartPowerMeter.Application
{
    public interface IEnergyMeasurementRepository
    {
        public Task AddMeasurementAsync(EnergyMeasurement measurement);
        public Task<IEnumerable<EnergyMeasurementDailyTotal>> GetDailyTotalMeasurementsLast30Days();
    }
}
