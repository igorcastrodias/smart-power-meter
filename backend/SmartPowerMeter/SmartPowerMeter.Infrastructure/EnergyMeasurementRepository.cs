using System;
using System.Data;
using System.Diagnostics.Metrics;
using Dapper;
using SmartPowerMeter.Application;
using SmartPowerMeter.Domain.Dtos;

namespace SmartPowerMeter.Infrastructure
{
	public class EnergyMeasurementRepository : IEnergyMeasurementRepository
    {
        private readonly IDbConnection _connection;

        public EnergyMeasurementRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task AddMeasurementAsync(EnergyMeasurement measurement)
        {
            var sql = @"
                INSERT INTO EnergyMeasurements 
                (DeviceId, Voltage, SensorVoltage, CurrentRMS, Power, Consumption, TimeStamp) 
                VALUES (@DeviceId, @Voltage, @SensorVoltage, @CurrentRMS, @Power, @Consumption, @TimeStamp)";

            await _connection.ExecuteAsync(sql, measurement);
        }

        public async Task<IEnumerable<EnergyMeasurementDailyTotal>> GetDailyTotalMeasurementsLast30Days()
        {
            var query = @"
        SELECT 
            DeviceId,
            CAST(TimeStamp AS DATE) AS Day, 
            SUM(Consumption) AS TotalConsumption
        FROM 
            EnergyMeasurements 
        WHERE 
            TimeStamp >= DATEADD(DAY, -30, GETDATE())
        GROUP BY 
            DeviceId,
            CAST(TimeStamp AS DATE)
        ORDER BY 
            DeviceId, 
            Day ASC";


            return await _connection.QueryAsync<EnergyMeasurementDailyTotal>(query);
        }
    }
}

