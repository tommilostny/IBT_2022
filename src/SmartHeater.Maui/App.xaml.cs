namespace SmartHeater.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = AppShellProvider.Create();
        }
    }
}