namespace SmartHeater.Maui.Pages;

public partial class WeatherPage
{
	public WeatherPage(CounterViewModel counterViewModel)
	{
		InitializeComponent();
		BindingContext = counterViewModel;
	}
}