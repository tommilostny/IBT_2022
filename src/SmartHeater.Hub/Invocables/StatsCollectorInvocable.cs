using Coravel.Invocable;
using SmartHeater.Hub.Providers;

namespace SmartHeater.Hub.Invocables;

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
        double? weather = null;
        foreach (var heater in await _heatersProvider.GetHeaterServicesAsync())
        {
            var heaterStatus = await heater.GetStatusAsync();
            if (heaterStatus is null)
            {
                Console.Error.WriteLine($"Could not get data from heater '{heater.IPAddress}'");
                continue;
            }
            weather ??= await _weatherService.ReadCelsiusAsync();
            var writtenToDb = _database.WriteMeasurement(heaterStatus, weather);
            Console.WriteLine(writtenToDb);
        }
    }
}
