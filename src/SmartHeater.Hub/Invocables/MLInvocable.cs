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
        foreach (var heater in await _heatersProvider.ReadHeatersAsync())
        {
            await SmartHeaterActionAsync(heater);
        }
    }

    private async Task SmartHeaterActionAsync(HeaterListModel heater)
    {
        //Get data from heater for model input.
        var heaterService = _heatersProvider.GetHeaterService(heater);
        if (heaterService is null)
        {
            Console.Error.WriteLine($"{heater.IpAddress}: Unable to load heater service.");
            return;
        }
        var status = await heaterService.GetStatusAsync();
        if (status?.Temperature is null)
        {
            Console.Error.WriteLine($"{heater.IpAddress}: Unable to load room temperature.");
            return;
        }
        var input = new ModelInput
        {
            TemperatureDiff = (float)status.Temperature - heater.ReferenceTemperature
        };

        //Perform forecasting
        var forecast = GetForecast(heater, input);

        //Check for over or under-heating.
        var average = forecast.TemperatureDiff.Average();
        var overheating = average > 2;
        var underheating = average < -2;
#if DEBUG
        Console.WriteLine($"Overheating: {overheating}");
        Console.WriteLine($"Underheating: {underheating}");
#endif

        //Control heater based on forecasted trend.
        //Upward trend (>0): turn off
        //Downward trend (<0): turn on
        var trend = ForecastingTrend(forecast);
        if (trend > 0 || overheating)
        {
#if DEBUG
            Console.WriteLine("sending off command");
#endif
            if (status.IsTurnedOn == true)
                await heaterService.TurnOffAsync();
        }
        else if (trend < 0 || underheating)
        {
#if DEBUG
            Console.WriteLine("sending on command");
#endif
            if (status.IsTurnedOn == false)
                await heaterService.TurnOnAsync();
        }
    }

    private static ModelOutput GetForecast(HeaterListModel heater, ModelInput input)
    {
#if DEBUG
        Console.WriteLine($"starting forecast for diff: {input.TemperatureDiff}");
#endif
        var forecast = SmartHeaterModel.Forecast(heater.IpAddress, input);
#if DEBUG
        Console.WriteLine("----------------------------------");
        foreach (var item in forecast.TemperatureDiff)
        {
            Console.WriteLine(item);
        }
#endif
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
#if DEBUG
        Console.WriteLine($"trend: {trend}");
#endif
        return trend;
    }
}
