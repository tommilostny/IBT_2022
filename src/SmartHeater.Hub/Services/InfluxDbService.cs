﻿using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

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
            .Measurement("heater_status")
            .Tag("heater", heater.IPAddress)
            .Field("temperature", heater.Temperature ?? double.NaN)
            .Field("weather", weather ?? double.NaN)
            .Field("power", heater.Power ?? double.NaN)
            .Timestamp(heater.MeasurementTime, WritePrecision.Ns);

        using var client = CreateDbClient();
        using var writeApi = client.GetWriteApi();
        writeApi.WritePoint(point, _bucket, _organization);

        return point.ToLineProtocol();
    }

    public async Task<IEnumerable<float>?> ReadHistoryAsync(HeaterListModel? heater, string period, string field)
    {
        if (heater is null || !HistoryFields.IsValid(field) || !HistoryPeriods.IsValid(period))
        {
            return null;
        }
        var query = $"from(bucket: \"{_bucket}\")"
                  + $" |> range(start: -{period})"
                  +  " |> filter(fn: (r) => r[\"_measurement\"] == \"heater_status\")"
                  + $" |> filter(fn: (r) => r[\"_field\"] == \"{field}\")"
                  + $" |> filter(fn: (r) => r[\"heater\"] == \"{heater.IpAddress}\")";
    
        using var client = CreateDbClient();
        var tables = await client.GetQueryApi().QueryAsync(query, _organization);
        var temperatures = new List<float>();

        foreach (var record in tables.SelectMany(table => table.Records))
        {
            temperatures.Add(Convert.ToSingle(record.GetValue()));
        }
        return temperatures;
    }

    private InfluxDBClient CreateDbClient()
    {
        return InfluxDBClientFactory.Create("http://localhost:8086", _token);
    }
}
