using Syncfusion.Maui.Core.Hosting;

namespace SmartHeater.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
            });

        builder.Services.AddSingleton<HeatersPage>();
        builder.Services.AddSingleton<WeatherPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddTransient<AddHeaterPage>();

        #if ANDROID || IOS
            builder.Services.AddTransient<HeaterDetailMobilePage>();
        #else
            builder.Services.AddTransient<HeaterDetailDesktopPage>();
        #endif

        builder.Services.AddSingleton<HeatersViewModel>();
        builder.Services.AddSingleton<WeatherViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddTransient<AddHeaterViewModel>();
        builder.Services.AddTransient<HeaterDetailViewModel>();

        builder.Services.AddSingleton(sp => SettingsProvider.LoadFromJson());
        builder.Services.AddSingleton(sp => new HttpClient { Timeout = TimeSpan.FromSeconds(5) });

        return builder.Build();
    }
}
