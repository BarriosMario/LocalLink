using CommunityToolkit.Mvvm.Messaging;

namespace LocalLink.View.Components;

public partial class AccountSettingsManager : ContentView
{
    public AccountSettingsManager()
    {
        InitializeComponent();

        // Registro del mensajero
        WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (m == "OpenSettingsMenu") await AbrirAnimacion();
                else if (m == "CloseSettingsMenu") await CerrarAnimacion();
            });
        });
    }

    private async Task AbrirAnimacion()
    {
        // 1. Hacemos visible el contenedor antes de animar
        this.IsVisible = true;
        this.InputTransparent = false;

        Overlay.IsVisible = true;

        // Ejecutamos en paralelo para mayor fluidez
        await Task.WhenAll(
            Overlay.FadeTo(1, 250, Easing.Linear),
            BottomMenu.TranslateTo(0, 0, 400, Easing.CubicOut)
        );
    }

    private async Task CerrarAnimacion()
    {
        await Task.WhenAll(
            Overlay.FadeTo(0, 250, Easing.Linear),
            // Asegúrate de que 800 sea suficiente para salir de pantalla
            BottomMenu.TranslateTo(0, 800, 350, Easing.CubicIn)
        );

        Overlay.IsVisible = false;
        this.InputTransparent = true;

        // 2. Ocultamos el componente completo para liberar recursos visuales
        this.IsVisible = false;
    }
}