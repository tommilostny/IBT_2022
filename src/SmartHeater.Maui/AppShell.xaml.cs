namespace SmartHeater.Maui;

public partial class AppShell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AddHeaterPage), typeof(AddHeaterPage));

		#if ANDROID || IOS
			Routing.RegisterRoute("HeaterDetailPage", typeof(HeaterDetailMobilePage));
		#else
			Routing.RegisterRoute("HeaterDetailPage", typeof(HeaterDetailDesktopPage));
		#endif
	}
}