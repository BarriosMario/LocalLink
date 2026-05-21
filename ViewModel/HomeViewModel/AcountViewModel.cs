using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LocalLink.Model;
using LocalLink.Services.Auth;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Data;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Storage;
using LocalLink.Services.Users;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LocalLink.ViewModel.HomeViewModel
{
    public class ProductSection
    {
        public string Title { get; set; }
        public ObservableCollection<Product> Products { get; set; } = new();
        public ICommand SeeMoreCommand { get; set; }
    }

    public partial class AcountViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IDeviceAccountManager _deviceAccountManager;
        private readonly IUserRepository _userRepository;
        private readonly IUserSettingsRepository _userSettingsRepository;

        [ObservableProperty]
        private User _currentUser;

        [ObservableProperty]
        private int _selectedLanguageIndex;

        public bool IsSellerEnabled => CurrentUser?.SellerProfile != null;

        public ObservableCollection<AccountSnapshot> Perfiles { get; set; } = new();

        // --- NUEVAS SECCIONES ---
        [ObservableProperty]
        private ProductSection _pedidosSection;

        [ObservableProperty]
        private ProductSection _favoritosSection;

        [ObservableProperty]
        private ProductSection _seguirComprandoSection;

        public AcountViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IAuthService authService,
            IDeviceAccountManager deviceAccountManager,
            IUserSettingsRepository userSettingsRepository,
            IUserRepository userRepository)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _authService = authService;
            _deviceAccountManager = deviceAccountManager;
            _userSettingsRepository = userSettingsRepository;
            _userRepository = userRepository;
        }

        public void RefreshUserData()
        {
            var usuarioGlobal = UserService.CurrentUser;
            if (usuarioGlobal != null)
            {
                CurrentUser = usuarioGlobal;

                // --- AGREGAR ESTO ---
                // Forzamos la notificación para que la UI sepa si es seller o no
                OnPropertyChanged(nameof(IsSellerEnabled));
                // --------------------

                if (CurrentUser.Settings != null)
                {
                    _selectedLanguageIndex = CurrentUser.Settings.PreferredLanguage == "en" ? 1 : 0;
                    OnPropertyChanged(nameof(SelectedLanguageIndex));
                    UpdateVisualTheme();
                }

                //_ = LoadAccountSections();
                _ = LoadAccounts();
            }
        }

        //private async Task LoadAccountSections()
        //{
        //    try
        //    {
        //        // 1. Mis Pedidos (Simulado o desde un Service de Compras)
        //        // Aquí deberías llamar a un servicio que traiga las compras del CurrentUser.Id
        //        PedidosSection = new ProductSection
        //        {
        //            Title = "Mis Pedidos",
        //            Products = new ObservableCollection<Product>() // Llena esto con datos de tu DB
        //        };

        //        // 2. Favoritos
        //        // Si el modelo User ya tiene favoritos, los usamos directamente
        //        FavoritosSection = new ProductSection
        //        {
        //            Title = "Favoritos",
        //            Products = new ObservableCollection<Product>(/* CurrentUser.Favorites ?? */ new List<Product>())
        //        };

        //        // 3. Seguir Comprando (Historial Local)
        //        // Puedes obtener esto de un servicio de "Recents" o Storage local
        //        SeguirComprandoSection = new ProductSection
        //        {
        //            Title = "Seguir comprando",
        //            Products = new ObservableCollection<Product>()
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error cargando secciones de cuenta: {ex.Message}");
        //    }
        //}

        [RelayCommand]
        private async Task AddProfile()
        {
            // 1. Validamos primero si ya alcanzó el límite físico del dispositivo
            var accounts = await _deviceAccountManager.GetAccountsAsync();
            if (accounts.Count >= 3)
            {
                await DialogService.ShowAlertAsync("Límite de cuentas",
                    "LocalLink permite un máximo de 3 perfiles vinculados por seguridad.", "Entendido");
                return;
            }

            // 2. Mostramos la confirmación al usuario
            bool deseaAnadir = await DialogService.ShowConfirmAsync(
                "Añadir cuenta",
                "¿Deseas vincular otra cuenta a este dispositivo?",
                "Confirmar",
                "Cancelar");

            if (deseaAnadir)
            {
                // Si confirma: Cerramos el menú y mandamos al Login
                CloseMenu();
                await GoLoginCommand.ExecuteAsync(null);
            }
            else
            {
                // Si cancela: Simplemente cerramos el menú desplegable
                CloseMenu();
            }
        }

        [RelayCommand]
        private async Task SwitchAccount(AccountSnapshot selected)
        {
            if (selected == null || selected.Email == CurrentUser?.Email) return;

            try
            {
                var fullUser = await _userRepository.GetUserByIdentifierAsync(selected.Email);
                if (fullUser != null)
                {
                    // --- AGREGAR ESTO: Cargar el perfil de la cuenta a la que te cambias ---
                    fullUser.SellerProfile = await _authService.GetSellerProfileAsync(fullUser.Id);
                    // ---------------------------------------------------------------------

                    await _authService.SaveLoginStatus(fullUser);
                    UserService.CurrentUser = fullUser;
                    CurrentUser = fullUser;

                    // Al refrescar, IsSellerEnabled se recalculará correctamente
                    RefreshUserData();
                    CloseMenu();
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error", "No se pudo cambiar de cuenta.", "OK");
            }
        }

        // --- PERSISTENCIA AUTOMÁTICA Y TEMA ---

        [RelayCommand]
        private async Task SaveSettingsDirectly()
        {
            if (CurrentUser?.Settings == null) return;

            try
            {
                // 1. Guardar en el almacenamiento seguro
                await _authService.SaveUserSettingsAsync(CurrentUser);

                // 2. Aplicar el tema visual inmediatamente
                UpdateVisualTheme();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en guardado automático: {ex.Message}");
            }
        }

        private void UpdateVisualTheme()
        {
            if (CurrentUser?.Settings == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.UserAppTheme = CurrentUser.Settings.DarkMode
                    ? AppTheme.Dark
                    : AppTheme.Light;
            });
        }

        // Se dispara automáticamente al cambiar el Picker
        partial void OnSelectedLanguageIndexChanged(int value)
        {
            if (CurrentUser?.Settings == null) return;

            CurrentUser.Settings.PreferredLanguage = (value == 1) ? "en" : "es";

            // Guardar cambio de idioma automáticamente
            _ = SaveSettingsDirectly();
        }

        [RelayCommand]
        private void CloseSettings()
        {
            // Ya no es estrictamente necesario guardar aquí porque los switches ya lo hacen,
            // pero lo dejamos como respaldo de seguridad.
            _ = SaveSettingsDirectly();
            WeakReferenceMessenger.Default.Send("CloseSettingsMenu");
        }

        // --- COMANDOS DE SESIÓN ---

        [RelayCommand]
        private async Task Logout()
        {
            bool confirm = await DialogService.ShowConfirmAsync(
                "Cerrar Sesión",
                "¿Estás seguro de que deseas quitar esta cuenta?",
                "Sí, quitar", "No");

            if (confirm)
            {
                await _authService.LogoutAsync(CurrentUser);
                if (UserService.CurrentUser != null)
                {
                    CurrentUser = UserService.CurrentUser;
                    RefreshUserData();
                }
                else
                {
                    await NavigationService.GoToAsync("//LoginPage");
                    WeakReferenceMessenger.Default.Send("CloseSettingsMenu");
                }
            }
        }

        // --- MÉTODOS DE SOPORTE ---

        public async Task LoadAccounts()
        {
            var snapshots = await _deviceAccountManager.GetAccountsAsync();
            if (snapshots == null) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Perfiles.Clear();
                var emailActual = CurrentUser?.Email ?? string.Empty;
                var listaPriorizada = snapshots
                    .OrderByDescending(s => s.Email.Equals(emailActual, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var s in listaPriorizada)
                    Perfiles.Add(s);
            });
        }

        [RelayCommand]
        private async Task EnableSeller()
        {
            // 1. Preguntar al usuario usando tu DialogService
            bool answer = await DialogService.ShowConfirmAsync(
                "Perfil de Vendedor",
                "¿Quieres convertirte en vendedor para empezar a publicar tus productos?",
                "Sí, quiero",
                "Tal vez luego");

            if (answer)
            {
                // 2. Llamamos al servicio para crear el perfil y persistirlo en sellers.json
                // El AuthService internamente usará la Factory y actualizará el CurrentUser
                bool success = await _authService.ActivateSellerProfileAsync(CurrentUser);

                if (success)
                {
                    // 3. Notificamos a la UI que 'IsSellerEnabled' ha cambiado.
                    // Al disparar esto, el XAML re-evalúa los botones y los intercambia.
                    OnPropertyChanged(nameof(IsSellerEnabled));

                    await DialogService.ShowAlertAsync("¡Felicidades!",
                        "Ahora puedes gestionar tu tienda desde el panel de configuración.", "OK");
                }
                else
                {
                    await DialogService.ShowAlertAsync("Error",
                        "No pudimos activar tu perfil en este momento.", "Intentar de nuevo");
                }
            }
        }

        [RelayCommand] private async Task OpenMenu() { await LoadAccounts(); WeakReferenceMessenger.Default.Send("OpenMenu"); }
        [RelayCommand] private void CloseMenu() => WeakReferenceMessenger.Default.Send("CloseMenu");
        [RelayCommand] private void OpenSettings() => WeakReferenceMessenger.Default.Send("OpenSettingsMenu");
        [RelayCommand] private async Task ManageStore() => await NavigationService.GoToAsync("StorePage");
    }
}