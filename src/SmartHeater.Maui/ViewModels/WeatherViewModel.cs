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
