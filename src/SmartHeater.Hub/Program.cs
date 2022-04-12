using Coravel;
using Microsoft.AspNetCore.HttpOverrides;
using SmartHeater.Hub.Services;
using SmartHeater.Hub.Invocables;

try
{
    await SmartHeaterModel.EnsureTrainedAsync(mlProjectPath: Path.Combine("..", "SmartHeater.ML"));
}
catch (Exception ex)
{
    Console.Error.WriteLine($"{DateTime.Now}: Unable to train machine learning model.");
    Console.Error.WriteLine($"Exception message: {ex.Message}");
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<StatsCollectorInvocable>();
builder.Services.AddTransient<MLInvocable>();
builder.Services.AddScheduler();

builder.Services.AddSingleton<IHeatersRepositoryService, HeatersRepositoryService>();
builder.Services.AddSingleton<IDatabaseService, InfluxDbService>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherService>();
builder.Services.AddSingleton<ICoordinatesService, IpApiService>();

builder.Services.AddSingleton(sp => new HttpClient
{
    Timeout = TimeSpan.FromSeconds(3)
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<StatsCollectorInvocable>().EveryTenSeconds();
    scheduler.Schedule<MLInvocable>().EveryMinute();
});

app.MapGet("/heaters/{ipAddress}/off",
    async (IHeatersRepositoryService hp, string ipAddress) =>
    {
        var service = await hp.GetHeaterServiceAsync(ipAddress);
        if (service is not null)
        {
            await service.TurnOffAsync();
        }
    }
);

app.MapGet("/heaters/{ipAddress}/on",
    async (IHeatersRepositoryService hp, string ipAddress) =>
    {
        var service = await hp.GetHeaterServiceAsync(ipAddress);
        if (service is not null)
        {
            await service.TurnOnAsync();
        }
    }
);

app.MapGet("/heaters",
    async (IHeatersRepositoryService hp)
        => await hp.ReadHeatersAsync()
);

app.MapGet("/heaters/{ipAddress}",
    async (IHeatersRepositoryService hp, string ipAddress)
        => await hp.GetHeaterDetailAsync(ipAddress)
);

app.MapPost("/heaters",
    async (IHeatersRepositoryService hp, HeaterListModel heater)
        => await hp.InsertAsync(heater)
);

app.MapPut("/heaters/{ipAddress}",
    async (IHeatersRepositoryService hp, string ipAddress, HeaterListModel heater)
        => await hp.UpdateAsync(ipAddress, heater)
);

app.MapDelete("/heaters/{ipAddress}",
    async (IHeatersRepositoryService hp, string ipAddress)
        => await hp.DeleteAsync(ipAddress)
);

app.MapGet("/weather",
    async (IWeatherService ws)
        => await ws.ReadCelsiusAsync()
);

app.MapGet("/heaters/{ipAddress}/history/{period}/{field}",
    async (IDatabaseService ds, IHeatersRepositoryService hp, string ipAddress, string period, string field)
        => await ds.ReadHistoryAsync(await hp.GetHeaterAsync(ipAddress), period, field)
);

app.MapGet("/periods",
    async () => await Task.FromResult(HistoryPeriods.GetAll())
);

app.MapGet("/fields",
    async () => await Task.FromResult(DbFields.GetAll())
);

app.MapGet("/smartheater-availability-test", () => "SmartHeater");

app.Run();
