using Coravel.Invocable;
using SmartHeater.Hub.Providers;

namespace SmartHeater.Hub.Invocables;

/// <summary>
/// Invocable task<br/>
/// <em>[0. Load registered heaters and for each of them do the following.]</em><br/>
/// 1. Create model (measurement from Shelly, OpenWeatherAPI, DateTime.UtcNow (once)).<br/>
/// 2. Load previous measurements from InfluxDB.<br/>
/// 3. Plug that into SmartHeater.ML.(...).Predict.<br/>
/// 4. Based on result call ShellyService turn on/off methods.<br/>
/// [5. Write measurement to InfluxDB (leave StatsCollectorInvocable running or replace it?)]
/// </summary>
public class MeasurementInvocable : IInvocable
{
    private readonly IDatabaseService _dbService;
    private readonly IWeatherService _weatherService;
    private readonly HeatersProvider _heatersProvider;

    public MeasurementInvocable(IDatabaseService databaseService, IWeatherService weatherService, HeatersProvider heatersProvider)
    {
        _dbService = databaseService;
        _weatherService = weatherService;
        _heatersProvider = heatersProvider;
    }

    public async Task Invoke()
    {
        //TODO: 1
        foreach (var heaterService in await _heatersProvider.GetHeaterServicesAsync())
        {
            //TODO: 2-4
        }
        throw new NotImplementedException();
    }
}
