using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class AcountPage : ContentPage
{
    public AcountPage(AcountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AcountViewModel vm)
        {
            vm.RefreshUserData();
        }
    }
}