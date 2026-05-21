using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Auth;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;

namespace LocalLink.ViewModel
{
    public partial class LoadingViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string loadingMessage = "Iniciando LocalLink...";

        [ObservableProperty]
        private string _currentTip;

        private readonly string[] _tips = new[]
        {
            "Apoya a los negocios locales de Yucatán.",
            "¿Sabías que puedes seguir a tus vendedores favoritos?",
            "Revisa siempre las valoraciones antes de comprar.",
            "LocalLink: Conectando emprendedores cerca de ti.",
            "Puedes activar el modo oscuro en los ajustes de tu cuenta.",
            "Gestiona tus propios productos volviéndote vendedor.",
            "Usa el buscador para encontrar exactamente lo que necesitas.",
            "¡Tu comunidad crece cuando compramos local!",
            "No olvides revisar la sección de 'Seguir comprando'.",
            "LocalLink es un proyecto hecho para impulsar el comercio."
        };

        private bool _isRotatingTips;

        public LoadingViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IAuthService authService)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _authService = authService;
        }

        private int _retryCount = 0; // Contador privado para rastrear reintentos

        [RelayCommand]
        private async Task Init()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // --- PASO 1: VERIFICACIÓN INICIAL DE RED ---
                bool hasInternet = ConnectivityService.IsConnected;

                if (hasInternet)
                {
                    LoadingMessage = "Estableciendo conexión...";
                    // Un delay muy corto solo para que el mensaje no parpadee
                    await Task.Delay(1000);
                }

                // --- PASO 2: LÓGICA DE REINTENTOS SI NO HAY RED ---
                if (!hasInternet)
                {
                    _retryCount++;
                    if (_retryCount >= 2)
                    {
                        bool modoOffline = await DialogService.ShowConfirmAsync(
                            "Sin conexión",
                            "No detectamos internet. ¿Deseas usar tus datos locales?",
                            "Modo Offline", "Reintentar");

                        if (modoOffline)
                        {
                            await ProcessUserSession("Iniciando en modo offline...");
                            return;
                        }
                        else { IsBusy = false; await Init(); return; }
                    }
                    else
                    {
                        bool retry = await DialogService.ShowConfirmAsync("Sin conexión", "No se pudo conectar.", "Reintentar", "Cerrar");
                        if (retry) { IsBusy = false; await Init(); return; }
                        else { Application.Current?.Quit(); return; }
                    }
                }

                // --- PASO 3: CARGA DINÁMICA (CON INTERNET) ---
                _retryCount = 0;
                // Llamamos al proceso que recupera los datos de forma real
                await ProcessUserSession("Estableciendo conexión y descargando datos...");
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error", ex.Message, "OK");
                StopTipsRotation();
                await GoLogin();
            }
            finally { IsBusy = false; }
        }

        private async Task ProcessUserSession(string initialMessage)
        {
            LoadingMessage = initialMessage;

            // Iniciamos la descarga de datos (la tarea real)
            var fetchTask = _authService.GetAuthenticatedUserAsync();

            // Mientras la tarea corre, podemos poner un delay mínimo de cortesía (UX)
            // para que la pantalla no salte demasiado rápido si el internet es fibra óptica.
            await Task.Delay(1500);

            // Esperamos a que la tarea de datos termine (aquí es donde se vuelve DINÁMICO)
            // Si el internet es lento, se quedará aquí hasta que termine.
            var user = await fetchTask;

            if (user != null)
            {
                StopTipsRotation();
                LoadingMessage = $"¡Listo! Bienvenido {user.Username}";
                UserService.CurrentUser = user;
                await Task.Delay(1000); // Un segundo final para que lean el "Bienvenido"
                await GoHome();
            }
            else
            {
                StopTipsRotation();
                await GoLogin();
            }
        }

        public async Task StartTipsRotation()
        {
            _isRotatingTips = true;
            Random random = new Random();
            int lastIndex = -1; // Guardamos el rastro del último tip mostrado

            while (_isRotatingTips)
            {
                int nextIndex;

                // Buscamos un índice nuevo que no sea igual al anterior
                do
                {
                    nextIndex = random.Next(_tips.Length);
                } while (nextIndex == lastIndex && _tips.Length > 1);

                CurrentTip = _tips[nextIndex];
                lastIndex = nextIndex;

                await Task.Delay(5000);
            }
        }

        public void StopTipsRotation() => _isRotatingTips = false;
    }
}