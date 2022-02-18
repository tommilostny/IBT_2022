namespace SmartHeater.Maui.ViewModels;

public class HeatersViewModel : BindableObject
{
    private readonly HttpClient _httpClient;

    public HeatersViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Load();
    }

    private ICommand _addCommand;
    public ICommand AddCommand => _addCommand ??= new Command(Add);

    private ICommand _loadCommand;
    public ICommand LoadCommand => _loadCommand ??= new Command(Load);

    public ObservableCollection<HeaterListModel> Heaters { get; } = new();

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

    public string LoadErrorMessage => "Unable to load heaters...";

    private async void Add()
    {
        await Shell.Current.GoToAsync(nameof(AddHeaterPage));
    }

    private void Load()
    {
        LoadError = false;
        Heaters.Clear();
        try
        {
            var heaters = _httpClient.GetFromJsonAsync<List<HeaterListModel>>("heaters").Result;
            foreach (var heater in heaters)
                Heaters.Add(heater);
        }
        catch
        {
            LoadError = true;
        }
    }
}
