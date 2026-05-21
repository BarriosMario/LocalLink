using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Auth;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Data;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;
using LocalLink.Services.Validation;

namespace LocalLink.ViewModel.LoginViewModel
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IValidationService _validationService;
        private readonly ILockoutService _lockoutService;
        private readonly IUserRepository _userRepository;

        // Token para cancelar el timer y liberar recursos de memoria
        private CancellationTokenSource _timerCts;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _identifier;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password;

        [ObservableProperty] private string _lockoutMessage;
        [ObservableProperty] private bool _isLockedOut;
        [ObservableProperty] private bool _showForgotPassword;

        public LoginViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IAuthService authService,
            IValidationService validationService,
            ILockoutService lockoutService,
            IUserRepository userRepository)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _authService = authService;
            _validationService = validationService;
            _lockoutService = lockoutService;
            _userRepository = userRepository;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            if (!ConnectivityService.IsConnected)
            {
                await DialogService.ShowAlertAsync("Sin conexión", "Revisa tu internet.", "Aceptar");
                return;
            }

            // OPTIMIZACIÓN: Liberar el token anterior antes de crear uno nuevo
            ResetTimerToken();
            _timerCts = new CancellationTokenSource();

            try
            {
                IsBusy = true;
                LoginCommand.NotifyCanExecuteChanged();

                // 1. Intento de Login (El servicio maneja la persistencia de intentos)
                var result = await _authService.LoginAsync(Identifier?.Trim(), Password);

                // 2. Sincronizar UI de ayuda (Botón "Olvidaste tu contraseña")
                UpdateHelpUI();

                switch (result)
                {
                    case LoginResult.Success:
                        await NavigationService.GoToAsync("//HomePage");
                        break;

                    case LoginResult.UserLocked:
                        var user = await _userRepository.GetUserByIdentifierAsync(Identifier?.Trim());
                        if (user?.LockoutEnd.HasValue == true)
                        {
                            await DialogService.ShowAlertAsync("Cuenta Bloqueada",
                                "Esta cuenta se encuentra bloqueada actualmente.", "Aceptar");

                            _ = StartLockoutTimer(user.LockoutEnd.Value, _timerCts.Token);
                        }
                        break;

                    case LoginResult.InvalidIdentifier:
                        string mensajeUser = Identifier.Contains("@")
                            ? "Cuenta no encontrada. Verifica el correo."
                            : "Usuario no encontrado. Revisa el nombre ingresado.";
                        await DialogService.ShowAlertAsync("Atención", mensajeUser, "Aceptar");
                        break;

                    case LoginResult.InvalidPassword:
                        UpdateHelpUI();
                        await DialogService.ShowAlertAsync("Contraseña Incorrecta",
                            "La contraseña ingresada no coincide con nuestros registros. Por favor, verifica tus datos e intenta de nuevo.",
                            "Aceptar");
                        break;

                    case LoginResult.Error:
                        await DialogService.ShowAlertAsync("Error", "Ocurrió un problema técnico.", "OK");
                        break;
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error Crítico", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                LoginCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanLogin()
        {
            if (IsBusy || IsLockedOut) return false;

            if (string.IsNullOrWhiteSpace(Identifier) || string.IsNullOrWhiteSpace(Password))
                return false;

            // Delegación total al ValidationService (Abstracción de Reglas de Negocio)
            var validation = _validationService.ValidateIdentifier(Identifier.Trim());

            return validation.IsValid && Password.Length >= 10;
        }

        private async Task StartLockoutTimer(DateTime expiration, CancellationToken ct)
        {
            IsLockedOut = true;
            LoginCommand.NotifyCanExecuteChanged();

            try
            {
                while (DateTime.UtcNow < expiration && !ct.IsCancellationRequested)
                {
                    // Delegación de formato al LockoutService
                    LockoutMessage = _lockoutService.GetFormattedRemainingTime(expiration);
                    await Task.Delay(1000, ct);
                }
            }
            catch (TaskCanceledException) { /* Timer cancelado por el usuario */ }

            if (!ct.IsCancellationRequested)
            {
                IsLockedOut = false;
                LockoutMessage = string.Empty;
                LoginCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnIdentifierChanged(string value)
        {
            ResetTimerToken();
            IsLockedOut = false;
            LockoutMessage = string.Empty;

            // Disparamos la tarea (Fire and Forget seguro en este contexto)
            UpdateHelpUI();

            LoginCommand.NotifyCanExecuteChanged();
        }

        // Método centralizado para evitar redundancia en el switch y el OnChanged
        private void UpdateHelpUI()
        {
            // Obtenemos los intentos actuales desde el servicio
            var attempts = _authService.GetCurrentAttempts(Identifier?.Trim());

            // Si lleva 3 o más, mostramos el botón de "Olvidé mi contraseña"
            ShowForgotPassword = attempts >= 3;
        }

        // Limpieza segura del token de cancelación
        private void ResetTimerToken()
        {
            if (_timerCts != null)
            {
                _timerCts.Cancel();
                _timerCts.Dispose();
                _timerCts = null;
            }
        }

        public void ResetFields()
        {
            Identifier = string.Empty;
            Password = string.Empty;
        }

        [RelayCommand]
        public async Task GoRegister() => await NavigationService.GoToAsync("RegisterPage");

        [RelayCommand]
        public async Task EmailVerification() => await NavigationService.GoToAsync("EmailVerification");
    }
}