namespace SmartHeater.Maui.ViewModels;

public class AddHeaterViewModel : BindableObject
{
    private readonly HttpClient _httpClient;
    private readonly HeatersViewModel _heatersViewModel;

    public AddHeaterViewModel(HttpClient httpClient, HeatersViewModel heatersViewModel)
    {
        _httpClient = httpClient;
        _heatersViewModel = heatersViewModel;
    }

    private ICommand _addHeaterCommand;
    public ICommand AddHeaterCommand => _addHeaterCommand ??= new Command(AddHeater);

    private string _ipAddress = string.Empty;
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            ShowError = false;
        }
    }

    private bool _showError = false;
    public bool ShowError
    {
        get => _showError;
        set
        {
            _showError = value;
            OnPropertyChanged(nameof(ShowError));
        }
    }

    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    private async void AddHeater()
    {
        try
        {
            //Check if input is a valid IP address, this throws FormatException if invalid.
            System.Net.IPAddress.Parse(_ipAddress);

            //Register valid heater IP address.
            await _httpClient.GetAsync($"heaters/add/{_ipAddress}");
            _heatersViewModel.Heaters.Add(_ipAddress);

            //HttpClient did not throw an exception => heater added successfully, go back.
            await Shell.Current.GoToAsync("..");
        }
        catch (FormatException)
        {
            ErrorMessage = $"'{_ipAddress}' is not valid IP address.";
            ShowError = true;
        }
        catch
        {
            ErrorMessage = "An error has occured while contacting the SmartHeater Hub.";
            ShowError = true;
        }
    }
}
