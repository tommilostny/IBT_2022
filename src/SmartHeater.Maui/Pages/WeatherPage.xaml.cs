namespace SmartHeater.Maui.Pages;

public partial class WeatherPage
{
	public WeatherPage(WeatherViewModel weatherViewModel)
	{
		InitializeComponent();
		BindingContext = weatherViewModel;
	}
}