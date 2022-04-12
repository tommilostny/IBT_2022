namespace SmartHeater.Maui.Pages;

public partial class SettingsPage
{
    public SettingsPage(SettingsViewModel settingsViewModel)
	{
		InitializeComponent();
		BindingContext = settingsViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await (BindingContext as SettingsViewModel).CheckAvailabilityAsync();
        }
        catch { }
    }
}
