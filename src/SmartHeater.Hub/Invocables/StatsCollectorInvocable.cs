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
        foreach (var heater in await _heatersProvider.GetHeaterServices())
        {
            var heaterStats = await heater.GetStatus();
            if (heaterStats is not null)
            {
                var weather = await _weatherService.ReadTemperatureC();
                var writtenToDb = _database.WriteMeasurement(heaterStats, weather);
                Console.WriteLine(writtenToDb);
                return;
            }
            Console.Error.WriteLine($"Could not get data from heater '{heater.IPAddress}'");
        }
    }
}
