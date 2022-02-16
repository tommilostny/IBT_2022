namespace SmartHeater.Maui.ViewModels;

public class HeatersViewModel : BindableObject
{
    private readonly HttpClient _httpClient;

    private ICommand _addCommand;
    public ICommand AddCommand => _addCommand ??= new Command(Add);

    private ICommand _loadCommand;
    public ICommand LoadCommand => _loadCommand ??= new Command(Load);

    public HeatersViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Load();
    }

    private List<string> _heaters;
    public List<string> Heaters
    {
        get => _heaters;
        set
        {
            _heaters = value;
            OnPropertyChanged();
        }
    }

    private void Add()
    {
        //Shell navigate to Add page
    }

    private void Load()
    {
        try
        {
            Heaters = _httpClient.GetFromJsonAsync<List<string>>("heaters").Result;
        }
        catch
        {
            var placeholders = new List<string>();
            var rand = new Random();
            for (int i = 0; i < 5; i++) placeholders.Add($"Placeholder {rand.Next()}");
            Heaters = placeholders;
        }
    }
}
