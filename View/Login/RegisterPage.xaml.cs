using LocalLink.ViewModel.LoginViewModel;

namespace LocalLink.View.Login;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}