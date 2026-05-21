using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Communications;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;

namespace LocalLink.ViewModel.LoginViewModel
{
    [QueryProperty(nameof(Email), "email")]
    public partial class GetCodeViewModel : BaseViewModel
    {
        private readonly IEmailService _emailService;
        private IDispatcherTimer _timer;

        [ObservableProperty] private string _email;
        [ObservableProperty] private string _textCorreo;
        [ObservableProperty] private string _timerText;

        // Añadimos NotifyCanExecuteChangedFor a todos los dígitos
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(VerifyCodeCommand))] private string _digit1;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(VerifyCodeCommand))] private string _digit2;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(VerifyCodeCommand))] private string _digit3;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(VerifyCodeCommand))] private string _digit4;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(VerifyCodeCommand))] private string _digit5;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(VerifyCodeCommand))] private string _digit6;

        public GetCodeViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IEmailService emailService)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _emailService = emailService;
            StartTimer();
        }

        partial void OnEmailChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
                TextCorreo = $"Hemos enviado un código a su correo {_emailService.MaskEmail(value)}";
        }

        private void StartTimer()
        {
            _timer?.Stop();
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);

            _timer.Tick += (s, e) =>
            {
                if (string.IsNullOrEmpty(Email)) return;

                var remaining = _emailService.GetRemainingTime(Email);
                TimerText = remaining.ToString(@"mm\:ss");

                if (remaining.TotalSeconds <= 0)
                {
                    _timer.Stop();
                    // Notifica para deshabilitar el botón cuando llega a cero
                    VerifyCodeCommand.NotifyCanExecuteChanged();
                    DialogService.ShowAlertAsync("Expirado", "El código ya no es válido.", "Aceptar");
                }
            };

            _timer.Start();

            // IMPORTANTE: Notifica al comando que el tiempo ha sido restaurado 
            // y el botón puede volver a habilitarse (si los dígitos están llenos)
            VerifyCodeCommand.NotifyCanExecuteChanged();
        }

        // 1. Añadimos el CanExecute al comando
        [RelayCommand(CanExecute = nameof(CanVerify))]
        private async Task VerifyCode()
        {
            string fullCode = $"{GetFirst(Digit1)}{GetFirst(Digit2)}{GetFirst(Digit3)}{GetFirst(Digit4)}{GetFirst(Digit5)}{GetFirst(Digit6)}";

            if (_emailService.VerifyCode(Email, fullCode))
            {
                _timer?.Stop();
                await NavigationService.GoToAsync($"ChangePassword?email={Email}");
            }
            else
            {
                await DialogService.ShowAlertAsync("Error", "Código incorrecto o expirado.", "Reintentar");
            }
        }

        // 2. Definimos la lógica de validación
        private bool CanVerify()
        {
            // Verificamos que ninguno esté vacío o nulo
            bool allFilled = !string.IsNullOrWhiteSpace(Digit1) &&
                             !string.IsNullOrWhiteSpace(Digit2) &&
                             !string.IsNullOrWhiteSpace(Digit3) &&
                             !string.IsNullOrWhiteSpace(Digit4) &&
                             !string.IsNullOrWhiteSpace(Digit5) &&
                             !string.IsNullOrWhiteSpace(Digit6);

            // Opcional: También podrías validar que el tiempo no sea 0
            var remaining = _emailService.GetRemainingTime(Email);

            return allFilled && remaining.TotalSeconds > 0;
        }

        private string GetFirst(string val) => string.IsNullOrEmpty(val) ? "" : val.Substring(0, 1);

        [RelayCommand]
        private async Task ResendCode()
        {
            await _emailService.SendVerificationCodeAsync(Email);
            StartTimer();
        }
    }
}