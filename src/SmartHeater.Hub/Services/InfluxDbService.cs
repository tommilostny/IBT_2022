using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Text;

namespace SmartHeater.Hub.Services;

public class InfluxDbService : IDatabaseService
{
    private readonly string _token;
    private readonly string _bucket;
    private readonly string _organization;

    public InfluxDbService(IConfiguration configuration)
    {
        _token = configuration["InfluxDB:Token"];
        _bucket = configuration["InfluxDB:Bucket"];
        _organization = configuration["InfluxDB:Organization"];
    }

    public string WriteMeasurement(HeaterStatusModel heater, double? weather)
    {
        var point = PointData
            .Measurement(DbFields.MeasurementName)
            .Tag(DbFields.HeaterTag, heater.IPAddress)
            .Field(DbFields.Temperature, heater.Temperature ?? double.NaN)
            .Field(DbFields.Weather, weather ?? double.NaN)
            .Field(DbFields.Power, heater.Power ?? double.NaN)
            .Timestamp(heater.MeasurementTime, WritePrecision.Ns);

        using var client = CreateDbClient();
        using var writeApi = client.GetWriteApi();
        writeApi.WritePoint(point, _bucket, _organization);

        return point.ToLineProtocol();
    }

    public async Task<ICollection<DbRecordModel>?> ReadHistoryAsync(HeaterListModel? heater, string period, string field)
    {
        if (heater is null || !DbFields.IsValid(field) || !HistoryPeriods.IsValid(period))
        {
            return null;
        }
        var queryBuilder = new StringBuilder()
            .Append($"from(bucket: \"{_bucket}\")")
            .Append($" |> range(start: -{period})")
            .Append($" |> filter(fn: (r) => r[\"_measurement\"] == \"{DbFields.MeasurementName}\")")
            .Append($" |> filter(fn: (r) => r[\"_field\"] == \"{field}\")")
            .Append($" |> filter(fn: (r) => r[\"{DbFields.HeaterTag}\"] == \"{heater.IpAddress}\")")
            .Append($" |> aggregateWindow(every: {HistoryPeriods.AggregationWindow(period)}, fn: mean, createEmpty: false)")
            .Append( " |> yield(name: \"mean\")");
    
        using var client = CreateDbClient();
        var tables = await client.GetQueryApi().QueryAsync(queryBuilder.ToString(), _organization);
        var measurements = new List<DbRecordModel>();

        foreach (var record in tables.SelectMany(table => table.Records))
        {
            measurements.Add(new DbRecordModel
            {
                Value = Convert.ToSingle(record.GetValue()),
                MeasurementTime = record.GetTimeInDateTime()
            });
        }
        return measurements;
    }

    private InfluxDBClient CreateDbClient()
    {
        return InfluxDBClientFactory.Create("http://localhost:8086", _token);
    }
}
