using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers; // Necesario para los handlers

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
                });

            // --- FORMA CORRECTA DE MODIFICAR EL ENTRY ---
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomEntry", (handler, view) =>
            {
#if ANDROID
                // Quita la línea inferior y el color de acento en Android
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
        // Quita el borde predeterminado en iOS/Mac
        handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
            // --- FIN DE CAMBIOS ---

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}