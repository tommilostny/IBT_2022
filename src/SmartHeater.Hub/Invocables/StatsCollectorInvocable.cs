using Coravel.Invocable;

namespace SmartHeater.Hub.Invocables;

public class StatsCollectorInvocable : IInvocable
{
    private readonly IHeatersRepositoryService _heatersProvider;
    private readonly IWeatherService _weatherService;
    private readonly IDatabaseService _database;

    public StatsCollectorInvocable(IHeatersRepositoryService heatersProvider, IWeatherService weatherService, IDatabaseService database)
    {
        _heatersProvider = heatersProvider;
        _weatherService = weatherService;
        _database = database;
    }

    public async Task Invoke()
    {
        var weather = await _weatherService.ReadCelsiusAsync();

        foreach (var heater in await _heatersProvider.GetHeaterServicesAsync())
        {
            await CollectHeaterDataAsync(heater, weather);
        }
    }

    private async Task CollectHeaterDataAsync(IHeaterControlService heater, double? weather)
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
