namespace SmartHeater.Maui.Pages;

public partial class HeaterDetailPage
{
	public HeaterDetailPage(HeaterDetailViewModel heaterDetailViewModel)
	{
		InitializeComponent();
		BindingContext = heaterDetailViewModel;
	}
}