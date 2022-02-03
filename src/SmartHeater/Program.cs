using Coravel;
using SmartHeater.Factories;
using SmartHeater.Invocables;
using SmartHeater.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<MeasurementInvocable>();
builder.Services.AddScheduler();

builder.Services.AddSingleton<IDatabaseService, InfluxDbService>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherService>();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<HeatersFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", () =>
{
    //var forecast = Enumerable.Range(1, 5).Select(index =>
    //   new WeatherForecast
    //   (
    //       DateTime.Now.AddDays(index),
    //       Random.Shared.Next(-20, 55),
    //       summaries[Random.Shared.Next(summaries.Length)]
    //   ))
    //    .ToArray();
    //return forecast;
})
.WithName("GetWeatherForecast");

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<MeasurementInvocable>().EveryMinute();
});

app.Run();
