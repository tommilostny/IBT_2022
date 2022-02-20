namespace SmartHeater.Maui.ViewModels;

public class HeatersViewModel : BindableObject
{
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;

    public HeatersViewModel(HttpClient httpClient, SettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        Load();
    }

    private ICommand _addCommand;
    public ICommand AddCommand => _addCommand ??= new Command(Add);

    private ICommand _loadCommand;
    public ICommand LoadCommand => _loadCommand ??= new Command(Load);

    private ICommand _selectionCommand;
    public ICommand SelectionCommand => _selectionCommand ??= new Command(ShowDetail);

    public ObservableCollection<HeaterListModel> Heaters { get; } = new();

    public HeaterListModel SelectedHeater { get; set; }

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

    public string LoadErrorMessage => string.IsNullOrWhiteSpace(_settingsProvider.HubIpAddress)
        ? "To load heaters, please, set up the Hub IP address in settings."
        : "Unable to load heaters.";

    private async void Add()
    {
        await Shell.Current.GoToAsync(nameof(AddHeaterPage));
    }

    private async void Load()
    {
        IsLoading = true;
        LoadError = false;
        Heaters.Clear();
        try
        {
            var uri = $"{_settingsProvider.HubUri}/heaters";
            var heaters = await _httpClient.GetFromJsonAsync<List<HeaterListModel>>(uri);
            foreach (var heater in heaters)
                Heaters.Add(heater);
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

    private async void ShowDetail()
    {
        if (SelectedHeater is not null)
        {
            var uri = $"{nameof(HeaterDetailPage)}?ipAddress={SelectedHeater.IpAddress}";
            await Shell.Current.GoToAsync(uri);
            DeselectSelectedHeater();
        }
    }

    private void DeselectSelectedHeater()
    {
        var heater = SelectedHeater;
        if (heater is not null)
        {
            var index = Heaters.IndexOf(heater);
            Heaters.RemoveAt(index);
            Heaters.Insert(index, heater);
        }
    }
}
