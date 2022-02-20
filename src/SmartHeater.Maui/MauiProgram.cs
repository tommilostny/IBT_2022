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
        builder.Services.AddTransient<AddHeaterPage>();
        builder.Services.AddTransient<HeaterDetailPage>();

        builder.Services.AddSingleton<HeatersViewModel>();
        builder.Services.AddSingleton<WeatherViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddTransient<AddHeaterViewModel>();
        builder.Services.AddTransient<HeaterDetailViewModel>();

        builder.Services.AddSingleton(sp => SettingsProvider.LoadFromJson());
        builder.Services.AddSingleton<HttpClient>();

        return builder.Build();
    }
}
