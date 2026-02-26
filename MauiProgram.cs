using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using LocalLink.Services;
using LocalLink.Repositories;
using LocalLink.Vistas;
using LocalLink.Login;

namespace LocalLink
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Montserrat-Regular.ttf", "Montserrat"); // Fuente nueva
                    fonts.AddFont("PlayfairDisplay-Bold.ttf", "PlayfairBold"); //Fuente nueva
                });

            // 🔥 Dependency Injection
            builder.Services.AddSingleton<EncryptionService>();
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<DeviceAccountManager>();

            builder.Services.AddTransient<AcountPage>();
            builder.Services.AddTransient<LoginPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // 🔧 Quitar línea inferior de Entry (forma correcta en MAUI)
#if ANDROID
            EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(
                        Android.Graphics.Color.Transparent);
            });
#endif

#if IOS || MACCATALYST
            EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                handler.PlatformView.BorderStyle =
                    UIKit.UITextBorderStyle.None;
            });
#endif

            return app;
        }
    }
}
