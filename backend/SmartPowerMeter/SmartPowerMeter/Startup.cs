using System;
using System.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SmartPowerMeter.Application;
using SmartPowerMeter.Infrastructure;
using System.Data.SqlClient;

[assembly: FunctionsStartup(typeof(SmartPowerMeter.Startup))]

namespace SmartPowerMeter
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get the connection string from the local.settings.json file.
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:SqlConnectionString");

            builder.Services.AddScoped<IDbConnection>(provider => new SqlConnection(connectionString));

            // Register the repository for dependency injection.
            builder.Services.AddScoped<IEnergyMeasurementRepository, EnergyMeasurementRepository>();
        }
    }
}
