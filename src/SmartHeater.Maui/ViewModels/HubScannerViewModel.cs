using SmartHeater.Maui.Helpers;

namespace SmartHeater.Maui.ViewModels;

public class HubScannerViewModel : BindableObject
{
    private readonly SettingsViewModel _settingsViewModel;
    private readonly HttpClient _httpClient;
    private readonly SettingsProvider _settingsProvider;

    public HubScannerViewModel(SettingsViewModel settingsViewModel, HttpClient httpClient, SettingsProvider settingsProvider)
    {
        _settingsViewModel = settingsViewModel;
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
    }

    private ICommand _autodiscoverCommand;
    public ICommand AutodiscoverCommand => _autodiscoverCommand ??= new Command(AutoDiscover);

    private ICommand _cancelCommand;
    public ICommand CancelCommand => _cancelCommand ??= new Command(Cancel);

    private string _subnetAddress = "192.168.1.0";
    public string SubnetAddress
    {
        get => _subnetAddress;
        set
        {
            _subnetAddress = value;
            OnPropertyChanged(nameof(SubnetAddress));
        }
    }

    private string _subnetMask = "24";
    public string SubnetMask
    {
        get => _subnetMask;
        set
        {
            _subnetMask = value;
            OnPropertyChanged(nameof(SubnetMask));
        }
    }

    private bool _scanning = false;
    public bool Scanning
    {
        get => _scanning;
        set
        {
            _scanning = value;
            OnPropertyChanged(nameof(Scanning));
        }
    }

    private CancellationTokenSource _tokenSource = null;

    private async void AutoDiscover()
    {
        _settingsViewModel.IsConnected = null;
        var ipAddress = _settingsViewModel.ParseIP(SubnetAddress);
        if (ipAddress is null)
            return;

        Scanning = true;
        _tokenSource = new();
        var subnet = new SubnetHelper(ipAddress, Convert.ToUInt16(SubnetMask));
        try
        {
            await Parallel.ForEachAsync(subnet.GetAllIpAddresses(), _tokenSource.Token, AvailabilityResultAsync);
        }
        catch
        {
        }
        if (_settingsViewModel.IsConnected != true)
        {
            _settingsViewModel.ErrorMessage = "Hub could not be found on the network.";
            _settingsViewModel.ShowError = true;
            _settingsViewModel.IsConnected = false;
        }
        Scanning = false;
        _tokenSource.Dispose();
        _tokenSource = null;
    }

    private async ValueTask AvailabilityResultAsync(string ipAddress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var reply = await _httpClient.GetAsync($"http://{ipAddress}/smartheater-availability-test", cancellationToken);
            if (reply.IsSuccessStatusCode && await reply.Content.ReadAsStringAsync(cancellationToken) == "SmartHeater")
            {
                _tokenSource.Cancel();
                _settingsViewModel.IsConnected = true;
                _settingsViewModel.HubIpAddress = ipAddress;
                _settingsProvider.SetHubAddress(ipAddress);
            }
        }
        catch
        {
        }
    }

    private void Cancel()
    {
        if (_tokenSource is not null && !_tokenSource.IsCancellationRequested)
        {
            _tokenSource.Cancel(true);
        }
    }
}
