using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class StorePage : ContentPage
{
	public StorePage(StoreViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Ejecutamos el comando de refresco cada vez que entramos a la pestaña "Cuenta"
        if (BindingContext is StoreViewModel vm)
        {
            vm.AppearingCommand.Execute(null);
        }
    }
}