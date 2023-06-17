using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using SmartPowerMeter.Application;
using SmartPowerMeter.Infrastructure;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("SqlConnectionString");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register the repository for dependency injection.
builder.Services.AddScoped<IDbConnection>(provider => new SqlConnection(connectionString));
builder.Services.AddScoped<IEnergyMeasurementRepository, EnergyMeasurementRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(x=>
    {
        x.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Smart Power Meter v1");
       
    });
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), ".well-known")),
    RequestPath = "/.well-known"
});

app.UseCors(x =>
{
    x.AllowAnyHeader();
    x.AllowAnyMethod();
    x.AllowAnyOrigin();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
