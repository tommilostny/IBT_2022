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

app.MapGet("/heaters/{ipAddress}/off",
    async (HeatersProvider hp, string ipAddress) =>
    {
        var service = await hp.GetHeaterServiceAsync(ipAddress);
        if (service is not null)
        {
            await service.TurnOffAsync();
        }
    }
);

app.MapGet("/heaters/{ipAddress}/on",
    async (HeatersProvider hp, string ipAddress) =>
    {
        var service = await hp.GetHeaterServiceAsync(ipAddress);
        if (service is not null)
        {
            await service.TurnOnAsync();
        }
    }
);

app.MapGet("/heaters",
    async (HeatersProvider hp)
        => await hp.ReadHeatersAsync()
);

app.MapGet("/heaters/{ipAddress}",
    async (HeatersProvider hp, string ipAddress)
        => await hp.GetHeaterDetailAsync(ipAddress)
);

app.MapPost("/heaters",
    async (HeatersProvider hp, HeaterListModel heater)
        => await hp.InsertAsync(heater)
);

app.MapPut("/heaters/{ipAddress}",
    async (HeatersProvider hp, string ipAddress, HeaterListModel heater)
        => await hp.UpdateAsync(ipAddress, heater)
);

app.MapDelete("/heaters/{ipAddress}",
    async (HeatersProvider hp, string ipAddress)
        => await hp.DeleteAsync(ipAddress)
);

app.MapGet("/weather",
    async (IWeatherService ws)
        => await ws.ReadCelsiusAsync()
);

app.Run();
