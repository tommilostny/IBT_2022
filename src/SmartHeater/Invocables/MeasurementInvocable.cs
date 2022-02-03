using Coravel.Invocable;
using SmartHeater.Factories;
using SmartHeater.Services.Interfaces;

namespace SmartHeater.Invocables;

/// <summary>
/// Invocable task<br/>
/// <em>[0. Load registered IP addresses (from DB(SELECT DISTINCT ip ...)) and for each of them do the following.]</em><br/>
/// 1. Create model (measurement from Shelly, OpenWeatherAPI, DateTime.Now (once)).<br/>
/// 2. Load previous measurements from InfluxDB.<br/>
/// 3. Plug that into SmartHeater.ML.(...).Predict.<br/>
/// 4. Based on result call ShellyService turn on/off methods.<br/>
/// 5. Write measurement to InfluxDB
/// </summary>
public class MeasurementInvocable : IInvocable
{
    private readonly IDatabaseService _influxDbService;
    private readonly IWeatherService _openWeatherService;
    private ICollection<IHeaterService> _heaters;

    public MeasurementInvocable(IDatabaseService influxDbService, IWeatherService openWeatherService, HeatersFactory heatersFactory)
    {
        _influxDbService = influxDbService;
        _openWeatherService = openWeatherService;
        _heaters = heatersFactory.GetHeaters();
    }

    public Task Invoke()
    {
        //TODO: 1
        foreach (var heater in _heaters)
        {
            //TODO: 2-5
        }

        throw new NotImplementedException();
    }
}
