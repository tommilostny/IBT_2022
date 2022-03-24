using Coravel.Invocable;

namespace SmartHeater.Hub.Invocables;

/// <summary>
/// Invocable task<br/>
/// <em>[0. Load registered heaters and for each of them do the following.]</em><br/>
/// 1. Create model (measurement from Shelly, OpenWeatherAPI, DateTime.UtcNow (once)).<br/>
/// 2. Load previous measurements from InfluxDB.<br/>
/// 3. Plug that into SmartHeater.ML.(...).Predict.<br/>
/// 4. Based on result call ShellyService turn on/off methods.<br/>
/// [5. Write measurement to InfluxDB (leave StatsCollectorInvocable running or replace it?)]
/// </summary>
public class StatsCollectorInvocable : IInvocable
{
    private readonly HeatersProvider _heatersProvider;
    private readonly IWeatherService _weatherService;
    private readonly IDatabaseService _database;

    public StatsCollectorInvocable(HeatersProvider heatersProvider, IWeatherService weatherService, IDatabaseService database)
    {
        _heatersProvider = heatersProvider;
        _weatherService = weatherService;
        _database = database;
    }

    public async Task Invoke()
    {
        var tasks = new List<Task>();
        var weather = await _weatherService.ReadCelsiusAsync();
        
        foreach (var heater in await _heatersProvider.GetHeaterServicesAsync())
        {
            tasks.Add(HeaterAction(heater, weather));
        }
        await Task.WhenAll(tasks);
    }

    private async Task HeaterAction(IHeaterService heater, double? weather)
    {
        var heaterStatus = await heater.GetStatusAsync();
        if (heaterStatus is null)
        {
            Console.Error.WriteLine($"Could not get data from heater '{heater.IPAddress}'");
            return;
        }
        var writtenToDb = _database.WriteMeasurement(heaterStatus, weather);
        Console.WriteLine(writtenToDb);
    }
}
