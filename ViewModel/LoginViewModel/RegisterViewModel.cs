using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Auth;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Security;
using LocalLink.Services.Users;
using LocalLink.Services.Validation;

namespace LocalLink.ViewModel.LoginViewModel
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IPasswordService _passwordService;
        private readonly IAuthService _authService;
        private readonly IValidationService _validationService;

        // Propiedades de Datos
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))] private string _email;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))] private string _username;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))] private string _password;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))] private string _confirmPassword;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))] private bool _isTermsAccepted;

        // Propiedades de Error para la UI
        [ObservableProperty] string _emailError;
        [ObservableProperty] string _usernameError;
        [ObservableProperty] string _passwordError;
        [ObservableProperty] string _confirmPasswordError;

        public RegisterViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IPasswordService passwordService,
            IAuthService authService,
            IValidationService validationService)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _passwordService = passwordService;
            _authService = authService;
            _validationService = validationService;
        }

        // Este es el comando que vinculaste en tu XAML
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveProfile()
        {
            if (!ConnectivityService.IsConnected)
            {
                await DialogService.ShowAlertAsync("Sin Conexión", "Revisa tu internet.", "Aceptar");
                return;
            }

            try
            {
                IsBusy = true;

                // Llamamos al nuevo método del servicio
                var (success, message) = await _authService.RegisterAsync(Username, Email, Password);

                if (success)
                {
                    await DialogService.ShowAlertAsync("¡Bienvenido!", "Tu cuenta ha sido creada con éxito.", "Aceptar");
                    // Navegamos al Home o Dashboard directamente porque ya tiene sesión iniciada
                    await NavigationService.GoToAsync("//LoginPage");
                }
                else
                {
                    await DialogService.ShowAlertAsync("Atención", message, "Intentar de nuevo");
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error Crítico", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave()
        {
            if (IsBusy) return false;

            if (_passwordService == null || _validationService == null) return false;

            // 1. Obtener valores limpios
            string email = Email ?? string.Empty;
            string user = Username ?? string.Empty;
            string pwd = Password ?? string.Empty;
            string confirm = ConfirmPassword ?? string.Empty;

            // 2. Ejecutar validaciones mediante servicios
            var vEmail = _validationService.ValidateEmail(email);
            var vUser = _validationService.ValidateUsername(user);
            var vPwd = _passwordService.ValidateComplexity(pwd);

            // 3. Gestión de Mensajes de Error (Feedback en tiempo real)
            EmailError = !string.IsNullOrEmpty(email) && !vEmail.IsValid ? vEmail.Message : string.Empty;
            UsernameError = !string.IsNullOrEmpty(user) && !vUser.IsValid ? vUser.Message : string.Empty;
            PasswordError = !string.IsNullOrEmpty(pwd) && !vPwd.IsValid ? vPwd.Message : string.Empty;

            // Error de Confirmación
            if (!string.IsNullOrEmpty(confirm))
            {
                ConfirmPasswordError = pwd != confirm ? "Las contraseñas tienen que ser iguales." : string.Empty;
            }
            else
            {
                ConfirmPasswordError = string.Empty;
            }

            // 4. Verificación de que todos los campos estén llenos
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(email) &&
                                   !string.IsNullOrWhiteSpace(user) &&
                                   !string.IsNullOrWhiteSpace(pwd) &&
                                   !string.IsNullOrWhiteSpace(confirm);

            // 5. El botón se activa solo si todo es perfecto y nada está vacío
            return allFieldsFilled && vEmail.IsValid && vUser.IsValid && vPwd.IsValid && pwd == confirm && IsTermsAccepted;
        }
    }
}
