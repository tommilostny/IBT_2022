using SmartHeater.Hub.Services;

namespace SmartHeater.Hub.Providers;

public class HeatersProvider
{
    private readonly HttpClient _httpClient;
    private const string _heatersJsonFile = "heaters.json";

    public HeatersProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ICollection<IHeaterService>> GetHeaterServices()
    {
        var services = new List<IHeaterService>();
        foreach (var heater in await ReadHeaters())
        {
            var heaterService = GetHeaterService(heater);
            if (heaterService is not null)
            {
                services.Add(heaterService);
            }
        }
        return services;
    }

    public IHeaterService? GetHeaterService(HeaterListModel heater)
    {
        return heater.HeaterType switch
        {
            HeaterTypes.Shelly1PM => new ShellyRelayService(_httpClient, heater.IpAddress),
            _ => null
        };
    }

    public async Task<ICollection<HeaterListModel>> Insert(HeaterListModel heater)
    {
        var heaters = await ReadHeaters();
        var existingHeater = heaters.FirstOrDefault(h => h.IpAddress == heater.IpAddress);

        if (existingHeater is null)
        {
            heaters.Add(heater);
            await WriteHeaters(heaters);
        }
        return heaters;
    }

    public async Task<ICollection<HeaterListModel>> Update(string originalIpAddress, HeaterListModel heater)
    {
        var heaters = (await ReadHeaters()).ToList();
        var existingHeater = heaters.FirstOrDefault(h => h.IpAddress == originalIpAddress);

        if (existingHeater is not null)
        {
            var index = heaters.IndexOf(existingHeater);
            heaters.RemoveAt(index);
            heaters.Insert(index, heater);
            await WriteHeaters(heaters);
        }
        return heaters;
    }

    public async Task<ICollection<HeaterListModel>> Delete(string ipAddress)
    {
        var heaters = await ReadHeaters();
        var heaterToRemove = heaters.FirstOrDefault(h => h.IpAddress == ipAddress);

        if (heaterToRemove is not null)
        {
            heaters.Remove(heaterToRemove);
            await WriteHeaters(heaters);
        }
        return heaters;
    }

    public async Task<HeaterDetailModel?> GetHeaterDetail(string ipAddress)
    {
        try
        {
            var heaterListModel = await GetHeaterListModel(ipAddress);
            var heaterService = GetHeaterService(heaterListModel);

            return new HeaterDetailModel(heaterListModel.IpAddress, heaterListModel.Name)
            {
                HeaterType = heaterListModel.HeaterType,
                ReferenceTemperature = heaterListModel.ReferenceTemperature,
                LastMeasurement = heaterService is not null ? await heaterService.GetStatus() : null
            };
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task<IHeaterService?> GetHeaterService(string ipAddress)
    {
        return GetHeaterService(await GetHeaterListModel(ipAddress));
    }

    private async Task<HeaterListModel> GetHeaterListModel(string ipAddress)
    {
        return (await ReadHeaters()).First(h => h.IpAddress == ipAddress);
    }

    public async Task<ICollection<HeaterListModel>> ReadHeaters()
    {
        var jsonStr = File.Exists(_heatersJsonFile) ? await File.ReadAllTextAsync(_heatersJsonFile) : null;
        return JsonConvert.DeserializeObject<List<HeaterListModel>>(jsonStr ?? "[]") ?? new();
    }

    private static async Task WriteHeaters(ICollection<HeaterListModel> heaters)
    {
        var jsonStr = JsonConvert.SerializeObject(heaters);
        await File.WriteAllTextAsync(_heatersJsonFile, jsonStr);
    }
}
