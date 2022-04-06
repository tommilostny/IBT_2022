namespace SmartHeater.Maui.Pages;

public partial class HeaterDetailDesktopPage : ContentPage
{
	public HeaterDetailDesktopPage(HeaterDetailViewModel heaterDetailViewModel)
	{
		InitializeComponent();
		BindingContext = heaterDetailViewModel;
	}
}