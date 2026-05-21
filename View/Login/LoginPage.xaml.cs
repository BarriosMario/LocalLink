using LocalLink.ViewModel.LoginViewModel;

namespace LocalLink.View.Login;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Aqui ocurre la conexión
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Accedemos al ViewModel a través del BindingContext
        if (BindingContext is LoginViewModel vm)
        {
            vm.ResetFields();
        }
    }
}