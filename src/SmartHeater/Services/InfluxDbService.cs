using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace SmartHeater.Services;

public class InfluxDbService : IDatabaseService
{
    private readonly string _token;
    private readonly string _organization;
    private readonly string _bucket;

    public InfluxDbService(IConfiguration configuration)
    {
        _token = configuration["InfluxDB:Token"];
        _bucket = configuration["InfluxDB:Bucket"];
        _organization = configuration["InfluxDB:Organization"];
    }

    public string WriteMeasurement(HeaterStatus heater, double weather)
    {
        var point = PointData
            .Measurement("heater_status")
            .Tag("heater", heater.IPAddress)
            .Field("temperature", heater.Temperature ?? double.NaN)
            .Field("weather", weather)
            .Field("power", heater.Power ?? double.NaN)
            .Field("turned_on", heater.IsTurnedOn ?? false)
            .Timestamp(heater.MeasurementTime, WritePrecision.Ns);

        using var client = InfluxDBClientFactory.Create("http://localhost:8086", _token);
        using var writeApi = client.GetWriteApi();
        writeApi.WritePoint(_bucket, _organization, point);

        return point.ToLineProtocol();
    }
}
