using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views; // Necesario para WindowInsetsController

namespace LocalLink
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 1. Cambiar el color de fondo de la barra
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#252525"));

            // 2. Cambiar el color de los iconos (Hora, Batería, WiFi)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                // Para Android 11 (API 30) o superior
                var windowInsetsController = Window.DecorView.WindowInsetsController;
                if (windowInsetsController != null)
                {
                    // false = Iconos blancos (para fondo oscuro)
                    // true = Iconos oscuros (para fondo claro)
                    windowInsetsController.SetSystemBarsAppearance(0, (int)WindowInsetsControllerAppearance.LightStatusBars);
                }
            }
            else
            {
                // Para versiones antiguas (API 23 a 29)
#pragma warning disable CS0618
                var decorView = Window.DecorView;
                var uiOptions = (int)decorView.SystemUiVisibility;
                // Al quitar el flag LightStatusBar, los iconos se vuelven blancos
                uiOptions &= ~(int)SystemUiFlags.LightStatusBar;
                decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
#pragma warning restore CS0618
            }
        }
    }
}