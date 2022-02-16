namespace SmartHeater.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<HeatersPage>();
        builder.Services.AddSingleton<WeatherPage>();
        builder.Services.AddSingleton<SettingsPage>();

        builder.Services.AddSingleton<CounterViewModel>();

        builder.Services.AddSingleton<HttpClient>();

        return builder.Build();
    }
}
