using System;

namespace SmartPowerMeter.Domain.Dtos
{
	public class EnergyMeasurement
	{
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public decimal Voltage { get; set; } = 127;
        public decimal SensorVoltage { get; set; }
        public decimal CurrentRMS { get; set; }
        public decimal Power { get; set; }
        public decimal Consumption { get; set; }
        public DateTime TimeStamp { get; set; }

        public EnergyMeasurement()
        {
            TimeStamp = DateTime.UtcNow;
        }
    }
}

