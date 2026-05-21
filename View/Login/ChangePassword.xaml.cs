using LocalLink.ViewModel.LoginViewModel;

namespace LocalLink.View.Login;

public partial class ChangePassword : ContentPage
{
    public ChangePassword(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Aqui ocurre la conexión
    }
}