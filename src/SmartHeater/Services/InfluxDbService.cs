using InfluxDB.Client;

namespace SmartHeater.Services;

public class InfluxDbService : IDatabaseService
{
    private readonly string _token;

    public InfluxDbService(IConfiguration configuration)
    {
        _token = configuration["InfluxDB:Token"];
    }

    public void Write(Action<WriteApi> action)
    {
        using var client = InfluxDBClientFactory.Create("http://localhost:8086", _token);
        using var write = client.GetWriteApi();
        action(write);
    }

    public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
    {
        using var client = InfluxDBClientFactory.Create("http://localhost:8086", _token);
        var query = client.GetQueryApi();
        return await action(query);
    }
}
