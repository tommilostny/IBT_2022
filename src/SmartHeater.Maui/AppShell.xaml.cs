namespace SmartHeater.Maui;

public partial class AppShell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AddHeaterPage), typeof(AddHeaterPage));
		Routing.RegisterRoute(nameof(HeaterDetailPage), typeof(HeaterDetailPage));
	}
}