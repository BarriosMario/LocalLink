using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class MenuPage : ContentPage
{
    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}