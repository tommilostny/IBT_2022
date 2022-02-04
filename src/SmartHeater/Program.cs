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

//app.Services.UseScheduler(scheduler =>
//{
//    scheduler.Schedule<MeasurementInvocable>().EveryMinute();
//});

var shelly = new ShellyRelayService(app.Services.GetService<HttpClient>()!, "192.168.1.253");

app.MapGet("/shelly/off", async () =>
{
    await shelly.TurnOff();
    return "Shelly turned off.";
})
.WithName("Turn off");

app.MapGet("/shelly/on", async () =>
{
    await shelly.TurnOn();
    return "Shelly turned on.";
})
.WithName("Turn on");

app.MapGet("/shelly/status", async () =>
{
    var status = await shelly.GetStatus();
    return $"Shelly status: {status}.";
})
.WithName("Get status");

app.MapGet("/shelly/temp", async () =>
{
    var temp = await shelly.ReadTemperature();
    return $"Measured temperature: {temp}°C.";
})
.WithName("Get temperature");

app.MapGet("/shelly/power", async () =>
{
    var power = await shelly.ReadPower();
    return $"Measured power: {power}W.";
})
.WithName("Get power");


app.Run();
