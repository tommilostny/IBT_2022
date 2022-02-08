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
builder.Services.AddTransient<StatsCollectorInvocable>();
builder.Services.AddScheduler();

builder.Services.AddSingleton<IDatabaseService, InfluxDbService>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherService>();
builder.Services.AddSingleton<IpApiService>();

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

app.Services.UseScheduler(scheduler =>
{
    //scheduler.Schedule<MeasurementInvocable>().EveryMinute();
    scheduler.Schedule<StatsCollectorInvocable>().EveryTenSeconds();
});

var shelly = app.Services.GetService<HeatersFactory>()!.GetHeaters().First();

app.MapGet("/shelly/off", async () =>
{
    await shelly.TurnOff();
    return "Shelly turned off.";
});

app.MapGet("/shelly/on", async () =>
{
    await shelly.TurnOn();
    return "Shelly turned on.";
});

app.MapGet("/shelly/status", async () => await shelly.GetStatus());

app.MapGet("/weather", async (IWeatherService weatherService) => await weatherService.ReadTemperatureC());

app.Run();
