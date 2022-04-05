namespace SmartHeater.Maui.ViewModels;

public class WeatherViewModel : BindableObject
{
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;

    public WeatherViewModel(HttpClient httpClient, SettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        Load();
    }

    private ICommand _reloadCommand;
    public ICommand ReloadCommand => _reloadCommand ??= new Command(Load);

    private bool _loadError = false;
    public bool LoadError
    {
        get => _loadError;
        set
        {
            _loadError = value;
            OnPropertyChanged(nameof(LoadError));
        }
    }

    private double _temperature;
    public double TemperatureC
    {
        get => _temperature;
        set
        {
            _temperature = value;
            OnPropertyChanged(nameof(TemperatureC));
        }
    }

    private bool _temperatureIsValid = false;
    public bool TemperatureIsValid
    {
        get => _temperatureIsValid;
        set
        {
            _temperatureIsValid = value;
            OnPropertyChanged(nameof(TemperatureIsValid));
        }
    }

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    public ObservableCollection<DbRecordModel> Data { get; } = new();

    private async void Load()
    {
        IsLoading = true;
        LoadError = false;
        TemperatureIsValid = false;
        try
        {
            var uri = $"{_settingsProvider.HubUri}/weather";
            TemperatureC = await _httpClient.GetFromJsonAsync<double>(uri);
            TemperatureIsValid = true;

            uri = $"{_settingsProvider.HubUri}/heaters/192.168.1.253/history/3h/weather";
            Data.Clear();
            foreach (var item in await _httpClient.GetFromJsonAsync<List<DbRecordModel>>(uri))
            {
                item.MeasurementTime = item.MeasurementTime.Value.ToLocalTime();
                Data.Add(item);
            }
        }
        catch
        {
            LoadError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
