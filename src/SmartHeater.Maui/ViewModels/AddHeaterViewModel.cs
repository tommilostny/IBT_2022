using SmartHeater.Maui.Helpers;

namespace SmartHeater.Maui.ViewModels;

public class AddHeaterViewModel : BindableObject, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private readonly HeatersViewModel _heatersViewModel;
    private readonly SettingsProvider _settingsProvider;

    public AddHeaterViewModel(HttpClient httpClient, HeatersViewModel heatersViewModel, SettingsProvider settingsProvider)
    {
        _httpClient = httpClient;
        _heatersViewModel = heatersViewModel;
        _settingsProvider = settingsProvider;
    }

    private ICommand _addHeaterCommand;
    public ICommand AddHeaterCommand => _addHeaterCommand ??= new Command(AddUpdateHeater);

    public HeaterTypes[] HeaterTypesList { get; } = (HeaterTypes[])Enum.GetValues(typeof(HeaterTypes));

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

    private float _refTemp = 23;
    public float ReferenceTemperature
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

    private string _saveIcon = IconFontHelper.ContentSaveOutline;
    public string SaveIcon
    {
        get => _saveIcon;
        set
        {
            _saveIcon = value;
            OnPropertyChanged(nameof(SaveIcon));
        }
    }

    private string _originalIpAddress = null;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.Keys.Count > 0)
        {
            IpAddress = query["IpAddress"].ToString();
            Name = query["Name"].ToString();
            HeaterType = (HeaterTypes)query["HeaterType"];
            ReferenceTemperature = (float)query["ReferenceTemperature"];

            Title = "Edit heater";
            ButtonText = "Save";
            SaveIcon = IconFontHelper.ContentSaveEditOutline;
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
