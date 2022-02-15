namespace SmartHeater.App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(CounterViewModel counterViewModel)
    {
        InitializeComponent();
        BindingContext = counterViewModel;
    }
}
