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

    //TODO: move from strings to HeaterListModel.
    public ObservableCollection<string> Heaters { get; } = new();

    private async void Add()
    {
        await Shell.Current.GoToAsync(nameof(AddHeaterPage));
    }

    private void Load()
    {
        Heaters.Clear();
        try
        {
            var heaters = _httpClient.GetFromJsonAsync<List<string>>("heaters").Result;
            foreach (var heater in heaters)
                Heaters.Add(heater);
        }
        catch
        {
            var rand = new Random();
            for (int i = 0; i < 5; i++)
                Heaters.Add($"Placeholder {rand.Next()}");
        }
    }
}
