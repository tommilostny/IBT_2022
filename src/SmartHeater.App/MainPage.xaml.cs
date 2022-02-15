namespace SmartHeater.App;

public partial class MainPage : ContentPage
{
    public MainPage(CounterViewModel counterViewModel)
    {
        InitializeComponent();
        BindingContext = counterViewModel;
    }
}
