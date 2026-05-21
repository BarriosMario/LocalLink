using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class AcountDetailsPage : ContentPage
{
    public AcountDetailsPage(AcountDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}