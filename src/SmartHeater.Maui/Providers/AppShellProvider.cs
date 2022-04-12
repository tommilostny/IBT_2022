namespace SmartHeater.Maui.Providers;

public static class AppShellProvider
{
    public static Shell Create()
    {
        Routing.RegisterRoute(nameof(AddHeaterPage), typeof(AddHeaterPage));

        #if ANDROID || IOS
            Routing.RegisterRoute("HeaterDetailPage", typeof(HeaterDetailMobilePage));
            return new MobileShell();
        #else
			Routing.RegisterRoute("HeaterDetailPage", typeof(HeaterDetailDesktopPage));
            return new DesktopShell();
        #endif
    }
}
