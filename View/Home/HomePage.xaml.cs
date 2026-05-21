using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Obtenemos el ViewModel desde el BindingContext
        if (BindingContext is HomeViewModel viewModel)
        {
            // 2. Ejecutamos la carga de productos de forma asíncrona
            // Esto actualizará AllProducts y disparará RefreshUI() automáticamente
            await viewModel.LoadProductsAsync();
        }
    }
}