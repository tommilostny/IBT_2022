using Coravel.Invocable;

namespace SmartHeater.Hub.Invocables;

public class MLInvocable : IInvocable
{
    private readonly IHeatersRepository _heatersRespository;

    public MLInvocable(IHeatersRepository heatersRepository)
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
            Console.Error.WriteLine($"{DateTime.Now}: {heater.IpAddress}: Unable to load heater service.");
            return;
        }
        var status = await heaterService.GetStatusAsync();
        if (status?.Temperature is null)
        {
            Console.Error.WriteLine($"{DateTime.Now}: {heater.IpAddress}: Unable to load room temperature.");
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
        GetDecisionMetrics(forecast, out var overheating, out var underheating, out var trend);

        if (overheating.Value || (trend.Value < 0 && !underheating.Value))
        {
            if (status.IsTurnedOn == true)
                await heaterService.TurnOffAsync();

            #if DEBUG
                Console.WriteLine("sending off command");
            #endif
        }
        else if (underheating.Value || (trend.Value > 0 && !overheating.Value))
        {
            if (status.IsTurnedOn == false)
                await heaterService.TurnOnAsync();

            #if DEBUG
                Console.WriteLine("sending on command");
            #endif
        }
    }

    private static void GetDecisionMetrics(ModelOutput forecast,
                                           out Lazy<bool> overheating,
                                           out Lazy<bool> underheating,
                                           out Lazy<float> trend)
    {
        //Compute over and under heating booleans.
        overheating  = new Lazy<bool>(() => forecast.TemperatureDiff.All(x => x > 0));
        underheating = new Lazy<bool>(() => forecast.TemperatureDiff.All(x => x < 0));

        //Compute the trend as the average value of differences between predicted values.
        trend = new Lazy<float>(() =>
        {
            float sum = .0f;
            int cnt = 0;
            do
            {
                sum += forecast.TemperatureDiff[cnt + 1] - forecast.TemperatureDiff[cnt];
            }
            while (++cnt < forecast.TemperatureDiff.Length - 1);
            return sum / cnt;
        });

        #if DEBUG
            Console.WriteLine($"Overheating: {overheating.Value}");
            Console.WriteLine($"Underheating: {underheating.Value}");
            Console.WriteLine($"Trend: {trend.Value}");
        #endif        
    }
}
