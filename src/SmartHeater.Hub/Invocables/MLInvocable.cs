using Coravel.Invocable;

namespace SmartHeater.Hub.Invocables;

public class MLInvocable : IInvocable
{
    private readonly IHeatersRepositoryService _heatersRespository;

    public MLInvocable(IHeatersRepositoryService heatersRepository)
    {
        _heatersRespository = heatersRepository;
    }

    public async Task Invoke()
    {
        foreach (var heater in await _heatersRespository.ReadHeatersAsync())
        {
            await SmartHeaterActionAsync(heater);
        }
    }

    private async Task SmartHeaterActionAsync(HeaterListModel heater)
    {
        //Get data from heater for model input.
        var heaterService = _heatersRespository.GetHeaterService(heater);
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
        var forecast = SmartHeaterModel.Forecast(heater.IpAddress, input);

        //Check for over or under-heating and get forecasted trend (function direction).
        //These metrics variables are used as follows:
        //  Overheating => turn off,
        //  Underheating => turn on,
        //  else if upward trend (>0) => turn on,
        //  else if downward trend (<0) => turn off.
        (var overheating, var underheating, var trend) = GetDecisionMetrics(forecast);

        if (overheating || (trend < 0 && !underheating))
        {
            if (status.IsTurnedOn == true)
                await heaterService.TurnOffAsync();

            #if DEBUG
                Console.WriteLine("sending off command");
            #endif
        }
        else if (underheating || (trend > 0 && !overheating))
        {
            if (status.IsTurnedOn == false)
                await heaterService.TurnOnAsync();

            #if DEBUG
                Console.WriteLine("sending on command");
            #endif
        }
    }

    private static (bool, bool, float) GetDecisionMetrics(ModelOutput forecast)
    {
        var overheating = forecast.TemperatureDiff.All(x => x > 0) && forecast.TemperatureDiff.Average() > 0.25;
        var underheating = forecast.TemperatureDiff.All(x => x < 0) && forecast.TemperatureDiff.Average() < -0.25;

        var diffs = new List<float>();
        for (int i = 0; i < forecast.TemperatureDiff.Length - 1; i++)
        {
            diffs.Add(forecast.TemperatureDiff[i + 1] - forecast.TemperatureDiff[i]);
        }
        var trend = diffs.Average();

        #if DEBUG
            Console.WriteLine($"Overheating: {overheating}");
            Console.WriteLine($"Underheating: {underheating}");
            Console.WriteLine($"Trend: {trend}");
        #endif
        
        return (overheating, underheating, trend);
    }
}
