using System.Web;

namespace SmartHeater.Maui.ViewModels;

public class AddHeaterViewModel : BindableObject, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private readonly HeatersViewModel _heatersViewModel;
    private readonly SettingsProvider _settingsProvider;

    public List<HeaterTypes> HeaterTypesList { get; } = new();

    public AddHeaterViewModel(HttpClient httpClient, HeatersViewModel heatersViewModel, SettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _heatersViewModel = heatersViewModel;
        _settingsProvider = settingsProvider;
        foreach (var item in Enum.GetValues(typeof(HeaterTypes)))
        {
            HeaterTypesList.Add((HeaterTypes)item);
        }
    }

    private ICommand _addHeaterCommand;
    public ICommand AddHeaterCommand => _addHeaterCommand ??= new Command(AddUpdateHeater);

    private string _ipAddress = string.Empty;
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            ShowError = false;
            OnPropertyChanged(nameof(IpAddress));
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            ShowError = false;
            OnPropertyChanged(nameof(Name));
        }
    }

    private HeaterTypes _type = HeaterTypes.Shelly1PM;
    public HeaterTypes HeaterType
    {
        get => _type;
        set
        {
            _type = value;
            ShowError = false;
            OnPropertyChanged(nameof(HeaterType));
        }
    }

    private double _refTemp = 23;
    public double ReferenceTemperature
    {
        get => _refTemp;
        set
        {
            _refTemp = value;
            OnPropertyChanged(nameof(ReferenceTemperature));
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

    private string _title = "Add heater";
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    private string _buttonText = "Add";
    public string ButtonText
    {
        get => _buttonText;
        set
        {
            _buttonText = value;
            OnPropertyChanged(nameof(ButtonText));
        }
    }

    private string _originalIpAddress = null;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.Keys.Count > 0)
        {
            IpAddress = HttpUtility.UrlDecode(query["IpAddress"].ToString());
            Name = HttpUtility.UrlDecode(query["Name"].ToString());
            HeaterType = (HeaterTypes)Convert.ToInt32(query["HeaterType"].ToString());
            ReferenceTemperature = Convert.ToDouble(query["ReferenceTemperature"].ToString());

            Title = "Edit heater";
            ButtonText = "Save";
            _originalIpAddress = IpAddress;
        }
    }

    private async void AddUpdateHeater()
    {
        try
        {
            //Check if input is a valid IP address, this throws FormatException if invalid.
            System.Net.IPAddress.Parse(_ipAddress);

            //Register heater with valid IP address.
            var heater = new HeaterListModel(_ipAddress, _name, _type, _refTemp);
            var uri = $"{_settingsProvider.HubUri}/heaters";
            var editingMode = !string.IsNullOrWhiteSpace(_originalIpAddress);

            //POST if adding new heater, PUT if editing a heater with set original IP address.
            var response = editingMode
                ? await _httpClient.PutAsJsonAsync($"{uri}/{_originalIpAddress}", heater)
                : await _httpClient.PostAsJsonAsync(uri, heater);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Unable to {(editingMode ? "edit" : "add")} heater.";
                ShowError = true;
                return;
            }
            //HttpClient did not throw an exception and status code was 200 OK.
            await _heatersViewModel.UpdateHeatersFromHttpAsync(response);

            //Heater added successfully, go back.
            //Pass possibly updated IP address if in editing mode.
            if (editingMode)
                await Shell.Current.GoToAsync($"..?ipAddress={_ipAddress}");
            else
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
