using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class ShoppingPage : ContentPage
{
    public ShoppingPage(ShoppingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}