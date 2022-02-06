using Coravel.Invocable;
using SmartHeater.Factories;

namespace SmartHeater.Invocables;

public class StatsCollectorInvocable : IInvocable
{
    private readonly ICollection<IHeaterService> _heaters;
    private readonly IWeatherService _weatherService;

    public StatsCollectorInvocable(HeatersFactory heatersFactory, IWeatherService weatherService)
    {
        _heaters = heatersFactory.GetHeaters();
        _weatherService = weatherService;
    }

    public async Task Invoke()
    {
        foreach (var heater in _heaters)
        {
            var heaterStats = await heater.GetStatus();
            var weather = await _weatherService.ReadTemperatureC();

            var statusText = $"\"{heaterStats.MeasurementTime}\",\"{heaterStats.Temperature}\",\"{weather}\",\"{heaterStats.Power}\",\"{heaterStats.IsTurnedOn}\"\n";

            Console.Write(statusText);
            await File.AppendAllTextAsync($"stats_{heaterStats.IPAddress}.csv", statusText);
        }
    }
}
