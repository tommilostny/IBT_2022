namespace SmartHeater.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = AppShellProvider.Create();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Title = "SmartHeater";
        return window;
    }
}
