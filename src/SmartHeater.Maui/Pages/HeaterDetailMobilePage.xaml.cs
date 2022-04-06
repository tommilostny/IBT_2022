namespace SmartHeater.Maui.Pages;

public partial class HeaterDetailMobilePage : ContentPage
{
	public HeaterDetailMobilePage(HeaterDetailViewModel heaterDetailViewModel)
	{
		InitializeComponent();
		BindingContext = heaterDetailViewModel;
	}
}