using Coravel.Invocable;
using SmartHeater.Factories;

namespace SmartHeater.Invocables;

public class StatsCollectorInvocable : IInvocable
{
    private readonly ICollection<IHeaterService> _heaters;
    private readonly IWeatherService _weatherService;
    private readonly IDatabaseService _database;

    public StatsCollectorInvocable(HeatersFactory heatersFactory, IWeatherService weatherService, IDatabaseService database)
    {
        _heaters = heatersFactory.GetHeaters();
        _weatherService = weatherService;
        _database = database;
    }

    public async Task Invoke()
    {
        foreach (var heater in _heaters)
        {
            var heaterStats = await heater.GetStatus();
            var weather = await _weatherService.ReadTemperatureC();

            var writtenToDb = _database.WriteMeasurement(heaterStats, weather);
            Console.WriteLine(writtenToDb);
        }
    }
}
