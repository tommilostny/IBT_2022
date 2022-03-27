using Coravel;
using SmartHeater.Hub.Services;
using SmartHeater.Hub.Invocables;

try
{
    await SmartHeaterModel.EnsureTrainedAsync(mlProjectPath: Path.Combine("..", "SmartHeater.ML"));
}
catch (Exception ex)
{
    Console.Error.WriteLine("Unable to train machine learning model.");
    Console.Error.WriteLine($"Exception message: {ex.Message}");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<StatsCollectorInvocable>();
builder.Services.AddTransient<MLInvocable>();
builder.Services.AddScheduler();

builder.Services.AddSingleton<IDatabaseService, InfluxDbService>();
builder.Services.AddSingleton<IWeatherService, OpenWeatherService>();
builder.Services.AddSingleton<ICoordinatesService, IpApiService>();
builder.Services.AddSingleton(sp => new HttpClient
{
    Timeout = TimeSpan.FromSeconds(3)
});

builder.Services.AddSingleton<IHeatersRepositoryService, HeatersRepositoryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.Services.UseScheduler(scheduler =>
//{
//    scheduler.Schedule<StatsCollectorInvocable>().EveryTenSeconds();
//    scheduler.Schedule<MLInvocable>().EveryMinute();
//});

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

app.MapGet("/heaters/{ipAddress}/temp-history",
    async (IDatabaseService ds, IHeatersRepositoryService hp, string ipAddress)
        => await ds.ReadTemperatureDiffsAsync(await hp.GetHeaterAsync(ipAddress))
);

app.Run();
