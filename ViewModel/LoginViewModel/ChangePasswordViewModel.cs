using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Communications;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Security;
using LocalLink.Services.Users;

namespace LocalLink.ViewModel.LoginViewModel
{
    [QueryProperty(nameof(Email), "email")]
    public partial class ChangePasswordViewModel : BaseViewModel
    {
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService; // Inyectado para la máscara

        [ObservableProperty] private string _email;
        [ObservableProperty] private string _maskedEmail; // Propiedad para la UI
        [ObservableProperty] private string _textChangePassword;

        // Propiedades con notificación para el CanExecute
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))] private string _newPassword;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))] private string _confirmPassword;

        // Propiedades de Error para feedback visual
        [ObservableProperty] private string _passwordError;
        [ObservableProperty] private string _confirmPasswordError;

        public ChangePasswordViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IPasswordService passwordService,
            IEmailService emailService) // Inyección de ambos servicios
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _passwordService = passwordService;
            _emailService = emailService;
        }

        partial void OnEmailChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Aplicamos la máscara de privacidad usando tu servicio
                MaskedEmail = _emailService.MaskEmail(value);
                TextChangePassword = $"Restableciendo contraseña para: {MaskedEmail}";
            }
        }

        [RelayCommand(CanExecute = nameof(CanChange))]
        private async Task ChangePassword()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Verificación de seguridad: No puede ser igual a la actual
                bool isSame = await UserService.IsSameAsPreviousPasswordAsync(Email, NewPassword);
                if (isSame)
                {
                    PasswordError = "No puedes usar la misma contraseña anterior.";
                    await DialogService.ShowAlertAsync("Contraseña Inválida", PasswordError, "Intentar otra");
                    return;
                }

                bool success = await UserService.UpdatePasswordAsync(Email, NewPassword);

                if (success)
                {
                    await DialogService.ShowAlertAsync("Éxito", "Tu contraseña ha sido actualizada correctamente.", "Aceptar");
                    await NavigationService.GoToAsync("///LoginPage");
                }
                else
                {
                    await DialogService.ShowAlertAsync("Error", "Hubo un problema al actualizar. Intenta más tarde.", "Aceptar");
                }
            }
            catch (Exception)
            {
                await DialogService.ShowAlertAsync("Error", "Ocurrió un error inesperado.", "Aceptar");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanChange()
        {
            // Bloquear si el sistema está procesando
            if (IsBusy) return false;

            string pwd = NewPassword ?? string.Empty;
            string confirm = ConfirmPassword ?? string.Empty;

            // 1. Validar complejidad con el servicio de contraseñas
            var vPwd = _passwordService.ValidateComplexity(pwd);

            // 2. Feedback de error en tiempo real para el usuario
            PasswordError = (!string.IsNullOrEmpty(pwd) && !vPwd.IsValid) ? vPwd.Message : string.Empty;

            if (!string.IsNullOrEmpty(confirm))
            {
                ConfirmPasswordError = (pwd != confirm) ? "Las contraseñas no coinciden." : string.Empty;
            }
            else
            {
                ConfirmPasswordError = string.Empty;
            }

            // 3. El botón se activa solo si: campos llenos + complejidad OK + coinciden
            bool isDataValid = !string.IsNullOrWhiteSpace(pwd) &&
                              !string.IsNullOrWhiteSpace(confirm) &&
                              vPwd.IsValid &&
                              pwd == confirm;

            return isDataValid;
        }

        [RelayCommand]
        private async Task Cancel() => await NavigationService.GoToAsync("///LoginPage");
    }
}