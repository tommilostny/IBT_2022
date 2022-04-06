using SmartHeater.Shared.Static;

namespace SmartHeater.Maui.ViewModels;

public class PeriodSelectorViewModel : BindableObject
{
    public PeriodSelectorViewModel(Action loadCommand)
    {
        LoadHistoryCommand = new Command(loadCommand);
    }

    public ICommand LoadHistoryCommand { get; }

    public string[] PeriodsList { get; } = HistoryPeriods.GetAll().ToArray();

    private string _selectedPeriod = HistoryPeriods.Hours3;
    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            _selectedPeriod = value;
            OnPropertyChanged(nameof(SelectedPeriod));
        }
    }
}
