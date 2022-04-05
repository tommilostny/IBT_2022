using Coravel.Invocable;

namespace SmartHeater.Hub.Invocables;

public class StatsCollectorInvocable : IInvocable
{
    private readonly IHeatersRepositoryService _heatersRepository;
    private readonly IWeatherService _weatherService;
    private readonly IDatabaseService _database;

    public StatsCollectorInvocable(IHeatersRepositoryService heatersRepository, IWeatherService weatherService, IDatabaseService database)
    {
        _heatersRepository = heatersRepository;
        _weatherService = weatherService;
        _database = database;
    }

    public async Task Invoke()
    {
        var weather = await _weatherService.ReadCelsiusAsync();

        foreach (var heater in await _heatersRepository.GetHeaterServicesAsync())
        {
            await CollectHeaterDataAsync(heater, weather);
        }
    }

    private async Task CollectHeaterDataAsync(IHeaterControlService heater, double? weather)
    {
        var heaterStatus = await heater.GetStatusAsync();
        if (heaterStatus is null)
        {
            Console.Error.WriteLine($"{DateTime.Now}: Could not get data from heater '{heater.IPAddress}'");
            return;
        }
        var writtenToDb = _database.WriteMeasurement(heaterStatus, weather);

        #if DEBUG
            Console.WriteLine(writtenToDb);
        #endif
    }
}
