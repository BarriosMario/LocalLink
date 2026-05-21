using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Communications;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Data; // Para IUserRepository
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;
using LocalLink.Services.Validation;

namespace LocalLink.ViewModel.LoginViewModel
{
    public partial class EmailVerificationViewModel : BaseViewModel
    {
        private readonly IEmailService _emailService;
        private readonly IValidationService _validationService;
        private readonly IUserRepository _userRepository; // Nuevo servicio

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendCodeCommand))]
        private string _email;

        public EmailVerificationViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IEmailService emailService,
            IValidationService validationService,
            IUserRepository userRepository) // Inyectado
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _emailService = emailService;
            _validationService = validationService;
            _userRepository = userRepository;
        }

        private bool CanSendCode() => _validationService.ValidateEmail(Email).IsValid;

        [RelayCommand(CanExecute = nameof(CanSendCode))]
        private async Task SendCode()
        {
            if (IsBusy) return;

            if (!ConnectivityService.IsConnected)
            {
                await DialogService.ShowAlertAsync("Sin conexión", "No tienes internet.", "Aceptar");
                return;
            }

            IsBusy = true;
            try
            {
                // 1. Buscamos al usuario por el correo ingresado
                var user = await _userRepository.GetUserByIdentifierAsync(Email);

                if (user == null)
                {
                    await DialogService.ShowAlertAsync("No encontrado",
                        "Este correo no está registrado en LocalLink.", "Aceptar");
                    return;
                }

                // 2. Si existe, generamos el código (el servicio hará el enmascaramiento y copiado)
                await _emailService.SendVerificationCodeAsync(user.Email);

                // 3. Navegamos pasando el objeto usuario o su ID para la siguiente fase
                // Usamos el Email ya normalizado que viene de la base de datos
                await NavigationService.GoToAsync($"GetCode?email={user.Email}");
            }
            catch (Exception)
            {
                await DialogService.ShowAlertAsync("Error", "Error al procesar la solicitud.", "Aceptar");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}