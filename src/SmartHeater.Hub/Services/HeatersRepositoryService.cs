namespace SmartHeater.Hub.Services;

public class HeatersRepositoryService : IHeatersRepositoryService
{
    private readonly HttpClient _httpClient;
    private const string _heatersJsonFile = "heaters.json";

    public HeatersRepositoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ICollection<IHeaterControlService>> GetHeaterServicesAsync()
    {
        var services = new List<IHeaterControlService>();
        foreach (var heater in await ReadHeatersAsync())
        {
            var heaterService = GetHeaterService(heater);
            if (heaterService is not null)
            {
                services.Add(heaterService);
            }
        }
        return services;
    }

    public IHeaterControlService? GetHeaterService(HeaterListModel heater)
    {
        return heater.HeaterType switch
        {
            HeaterTypes.Shelly1PM => new ShellyRelayService(_httpClient, heater.IpAddress),
            _ => null
        };
    }

    public async Task<ICollection<HeaterListModel>> InsertAsync(HeaterListModel heater)
    {
        var heaters = await ReadHeatersAsync();
        var existingHeater = heaters.FirstOrDefault(h => h.IpAddress == heater.IpAddress);

        if (existingHeater is null)
        {
            heaters.Add(heater);
            await WriteHeatersAsync(heaters);
        }
        return heaters;
    }

    public async Task<ICollection<HeaterListModel>> UpdateAsync(string originalIpAddress, HeaterListModel heater)
    {
        var heaters = (await ReadHeatersAsync()).ToList();
        var existingHeater = heaters.FirstOrDefault(h => h.IpAddress == originalIpAddress);

        if (existingHeater is not null)
        {
            var index = heaters.IndexOf(existingHeater);
            heaters.RemoveAt(index);
            heaters.Insert(index, heater);
            await WriteHeatersAsync(heaters);
        }
        return heaters;
    }

    public async Task<ICollection<HeaterListModel>> DeleteAsync(string ipAddress)
    {
        var heaters = await ReadHeatersAsync();
        var heaterToRemove = heaters.FirstOrDefault(h => h.IpAddress == ipAddress);

        if (heaterToRemove is not null)
        {
            heaters.Remove(heaterToRemove);
            await WriteHeatersAsync(heaters);
        }
        return heaters;
    }

    public async Task<HeaterDetailModel?> GetHeaterDetailAsync(string ipAddress)
    {
        try
        {
            var heaterListModel = await GetHeaterAsync(ipAddress);
            var heaterService = GetHeaterService(heaterListModel);

            return new HeaterDetailModel(heaterListModel.IpAddress, heaterListModel.Name)
            {
                HeaterType = heaterListModel.HeaterType,
                ReferenceTemperature = heaterListModel.ReferenceTemperature,
                LastMeasurement = heaterService is not null ? await heaterService.GetStatusAsync() : null
            };
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task<IHeaterControlService?> GetHeaterServiceAsync(string ipAddress)
    {
        var heater = await GetHeaterAsync(ipAddress);
        if (heater is null)
        {
            return null;
        }
        return GetHeaterService(heater);
    }

    public async Task<HeaterListModel?> GetHeaterAsync(string ipAddress)
    {
        return (await ReadHeatersAsync()).FirstOrDefault(h => h.IpAddress == ipAddress);
    }

    public async Task<ICollection<HeaterListModel>> ReadHeatersAsync()
    {
        var jsonStr = File.Exists(_heatersJsonFile) ? await File.ReadAllTextAsync(_heatersJsonFile) : null;
        return JsonConvert.DeserializeObject<List<HeaterListModel>>(jsonStr ?? "[]") ?? new();
    }

    public async Task WriteHeatersAsync(ICollection<HeaterListModel> heaters)
    {
        var jsonStr = JsonConvert.SerializeObject(heaters);
        await File.WriteAllTextAsync(_heatersJsonFile, jsonStr);
    }
}
