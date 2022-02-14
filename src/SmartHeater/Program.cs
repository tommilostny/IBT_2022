using Coravel;
using SmartHeater.Providers;
using SmartHeater.Invocables;
using SmartHeater.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<MeasurementInvocable>();
builder.Services.AddTransient<StatsCollectorInvocable>();
builder.Services.AddScheduler();

builder.Services.AddSingleton<IDatabaseService, InfluxDbService>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherService>();
builder.Services.AddSingleton<IpApiService>();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<HeatersProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.Services.UseScheduler(scheduler =>
//{
//    //scheduler.Schedule<MeasurementInvocable>().EveryMinute();
//    scheduler.Schedule<StatsCollectorInvocable>().EveryTenSeconds();
//});

var shelly = new ShellyRelayService(new(), "192.168.1.253");

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

app.MapGet("/heaters", async (HeatersProvider hp) => await hp.GetHeaters());
app.MapGet("/heaters/add/{ipAddress}", async (HeatersProvider hp, string ipAddress) => await hp.Register(ipAddress));
app.MapGet("/heaters/remove/{ipAddress}", async (HeatersProvider hp, string ipAddress) => await hp.Remove(ipAddress));

app.Run();
