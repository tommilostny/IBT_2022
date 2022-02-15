namespace SmartHeater.App.ViewModels;

public class CounterViewModel : BindableObject
{
    ICommand _clickedCommand;
    public ICommand ClickedCommand => _clickedCommand ??= new Command(IncrementCount);

    private void IncrementCount()
    {
        Count++;
    }

    private int _count = 0;
    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            OnPropertyChanged();
        }
    }
}
