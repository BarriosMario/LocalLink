using LocalLink.ViewModel.LoginViewModel;

namespace LocalLink.View.Login;

public partial class EmailVerification : ContentPage
{
    public EmailVerification(EmailVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Aqui ocurre la conexión
    }
}