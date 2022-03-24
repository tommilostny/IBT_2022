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
        //Get data from heater for model input.
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
        var input = new ModelInput
        {
            TemperatureDiff = (float)status.Temperature - heater.ReferenceTemperature
        };

        //Perform forecasting
        var forecast = ForecastAndPrint(heater, input);

        //Control heater based on forecasted trend.
        //Upward trend (>0): turn off
        //Downward trend (<=0): turn on
        var trend = ForecastingTrend(forecast);
        if (trend > 0 && status?.IsTurnedOn == true)
        {
            Console.WriteLine("sending off command");
            await heaterService.TurnOffAsync();
        }
        else if (trend < 0 && status?.IsTurnedOn == false)
        {
            Console.WriteLine("sending on command");
            await heaterService.TurnOnAsync();
        }
    }

    private static ModelOutput ForecastAndPrint(HeaterListModel heater, ModelInput input)
    {
        Console.WriteLine($"starting forecast for diff: {input.TemperatureDiff}");

        var forecast = SmartHeaterModel.Forecast(heater.IpAddress, input);

        Console.WriteLine("----------------------------------");
        foreach (var item in forecast.TemperatureDiff)
        {
            Console.WriteLine(item);
        }
        return forecast;
    }

    private static float ForecastingTrend(ModelOutput forecast)
    {
        var diffs = new List<float>();
        for (int i = 0; i < forecast.TemperatureDiff.Length - 1; i++)
        {
            diffs.Add(forecast.TemperatureDiff[i + 1] - forecast.TemperatureDiff[i]);
        }
        var trend = diffs.Average();
        Console.WriteLine($"trend: {trend}");
        return trend;
    }
}
