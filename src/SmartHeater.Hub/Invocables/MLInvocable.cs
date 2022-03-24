using Coravel.Invocable;

namespace SmartHeater.Hub.Invocables;

public class MLInvocable : IInvocable
{
    private readonly HeatersProvider _heatersProvider;

    public MLInvocable(HeatersProvider heatersProvider)
    {
        _heatersProvider = heatersProvider;
    }

    public async Task Invoke()
    {
        var tasks = new List<Task>();
        foreach (var heater in await _heatersProvider.ReadHeatersAsync())
        {
            tasks.Add(ForecastHeaterAction(heater));
        }
        await Task.WhenAll(tasks);
    }

    private async Task ForecastHeaterAction(HeaterListModel heater)
    {
        var heaterService = _heatersProvider.GetHeaterService(heater);
        if (heaterService is null)
        {
            return;
        }
        var status = await heaterService.GetStatusAsync();
        if (status?.Temperature is null)
        {
            return;
        }
        var input = new MLModelInput
        {
            TemperatureDiff = (float)status.Temperature - heater.ReferenceTemperature
        };

        var forecast = SmartHeaterModel.Forecast(heater.IpAddress, input);

        Console.WriteLine("----------------------------------");
        foreach (var item in forecast.TemperatureDiff)
        {
            Console.WriteLine(item);
        }
    }
}
