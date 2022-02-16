namespace SmartHeater.Maui.Pages;

public partial class HeatersPage
{
	public HeatersPage(HeatersViewModel heatersViewModel)
	{
		InitializeComponent();
		BindingContext = heatersViewModel;
	}
}
