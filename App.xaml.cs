using LocalLink.Services.Auth;
using LocalLink.Services.Users;

namespace LocalLink
{
    public partial class App : Application
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public App(
            IAuthService authService,
            IUserService userService)
        {
            InitializeComponent();

            _authService = authService;
            _userService = userService;

            // 1. Establecemos el Shell inmediatamente
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Al iniciar, delegamos la responsabilidad a la pantalla de carga
            await CheckAuthAndNavigate();
        }

        #region Lógica de Autenticación y Navegación

        private async Task CheckAuthAndNavigate()
        {
            // 1. Pequeño delay para asegurar que Shell esté listo
            await Task.Delay(100);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    // 2. NAVEGACIÓN INICIAL OBLIGATORIA A LOADING
                    // Esto asegura que lo primero que vea el usuario sea tu nueva vista de carga
                    await Shell.Current.GoToAsync("//LoadingPage");
                }
                catch (Exception)
                {
                    // Si por alguna razón falla el redireccionamiento inicial, 
                    // intentamos ir al Login por seguridad.
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            });
        }

        #endregion
    }
}