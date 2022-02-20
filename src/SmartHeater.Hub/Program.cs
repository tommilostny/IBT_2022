using Coravel;
using SmartHeater.Hub.Services;
using SmartHeater.Hub.Invocables;
using SmartHeater.Hub.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<MeasurementInvocable>();
builder.Services.AddTransient<StatsCollectorInvocable>();
builder.Services.AddScheduler();

builder.Services.AddSingleton<IDatabaseService, InfluxDbService>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherService>();
builder.Services.AddSingleton<ICoordinatesService, IpApiService>();

builder.Services.AddSingleton(sp => new HttpClient
{
    Timeout = TimeSpan.FromSeconds(3)
});
builder.Services.AddSingleton<HeatersProvider>();

var app = builder.Build();

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

var shelly = new ShellyRelayService(app.Services.GetService<HttpClient>()!, "192.168.1.253");

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

app.MapGet("/shelly/status",
    async () => await shelly.GetStatus());

app.MapGet("/weather",
    async (IWeatherService ws) => await ws.ReadTemperatureC());

app.MapGet("/heaters",
    async (HeatersProvider hp) => await hp.ReadHeaters());

app.MapGet("/heaters/{ipAddress}",
    async (HeatersProvider hp, string ipAddress) => await hp.GetHeaterDetail(ipAddress));

app.MapPost("/heaters",
    async (HeatersProvider hp, HeaterListModel heater) => await hp.InsertUpdate(heater));

app.MapDelete("/heaters/{ipAddress}",
    async (HeatersProvider hp, string ipAddress) => await hp.Delete(ipAddress));

app.Run();
