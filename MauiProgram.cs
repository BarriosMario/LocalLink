using CommunityToolkit.Maui;
using LocalLink.Services.Auth;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Data;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Storage;
using LocalLink.View.Home;
using LocalLink.View.Login;
using LocalLink.View;
using LocalLink.ViewModel;
using LocalLink.ViewModel.HomeViewModel;
using LocalLink.ViewModel.LoginViewModel;
using Microsoft.Extensions.Logging;
using LocalLink.Services.Security;
using LocalLink.Services.Communications;
using LocalLink.Services.Validation;
using LocalLink.Services.Users;

namespace LocalLink
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // --- 1. REGISTRO DE SERVICIOS COMPARTIDOS ---

            // 1.1 Autenticación (Usando la Interfaz que creaste)
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IProductService, ProductService>();

            //Interfaz de navegacion entre vistas
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // Registro de repositorios con interfaces
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<ISellerRepository, SellerRepository>();
            builder.Services.AddSingleton<IProductRepository, ProductRepository>();
            builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
            builder.Services.AddSingleton<IChatRepository, ChatRepository>();
            builder.Services.AddSingleton<IUserFactory, UserFactory>();
            builder.Services.AddSingleton<IUserSettingsRepository, UserSettingsRepository>();

            // 1.2 Otros servicios de Auth
            builder.Services.AddSingleton<ILockoutService, LockoutService>();
            builder.Services.AddSingleton<IPasswordService, PasswordService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();

            // 1.3 Almacenamiento y Seguridad
            builder.Services.AddSingleton<EncryptionService>();
            builder.Services.AddSingleton<IDeviceAccountManager, DeviceAccountManager>();

            // 1.4 Alertas, ventanas de confirmación y conexión
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

            // En MauiProgram.cs dentro de CreateMauiApp()
            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<LoadingViewModel>();

            // --- 2. REGISTRO DE VIEWMODELS ---
            // Login Flow
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<EmailVerificationViewModel>();
            builder.Services.AddTransient<GetCodeViewModel>();
            builder.Services.AddTransient<ChangePasswordViewModel>();

            // Home Flow
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<ShoppingViewModel>();
            builder.Services.AddSingleton<AcountViewModel>();
            builder.Services.AddSingleton<MenuViewModel>();
            builder.Services.AddTransient<AcountDetailsViewModel>();
            builder.Services.AddTransient<StoreViewModel>();
            builder.Services.AddTransient<AddProductViewModel>();

            // --- 3.  REGISTRO DE VIEWS ---
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<EmailVerification>();
            builder.Services.AddTransient<GetCode>();
            builder.Services.AddTransient<ChangePassword>();

            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<ShoppingPage>();
            builder.Services.AddSingleton<AcountPage>();
            builder.Services.AddSingleton<MenuPage>();
            builder.Services.AddTransient<AcountDetailsPage>();
            builder.Services.AddTransient<StorePage>();
            builder.Services.AddTransient<AddProductPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
