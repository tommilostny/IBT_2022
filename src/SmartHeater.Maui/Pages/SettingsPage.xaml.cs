namespace SmartHeater.Maui.Pages;

public partial class SettingsPage
{
	public SettingsPage(SettingsViewModel settingsViewModel)
	{
		InitializeComponent();
		BindingContext = settingsViewModel;
	}
}