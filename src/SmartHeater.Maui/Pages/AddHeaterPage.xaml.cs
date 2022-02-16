namespace SmartHeater.Maui;

public partial class AddHeaterPage
{
	public AddHeaterPage(AddHeaterViewModel addHeaterViewModel)
	{
		InitializeComponent();
		BindingContext = addHeaterViewModel;
	}
}