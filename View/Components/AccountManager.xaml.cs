using CommunityToolkit.Mvvm.Messaging;

namespace LocalLink.View.Components;

public partial class AccountManager : ContentView
{
    // Definimos las referencias manualmente
    private BoxView overlayControl;
    private Border menuControl;

    public AccountManager()
    {
        InitializeComponent();

        // Buscamos los controles por el x:Name que pusiste en el XAML
        overlayControl = this.FindByName<BoxView>("Overlay");
        menuControl = this.FindByName<Border>("BottomMenu");

        WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (m == "OpenMenu") await AbrirAnimacion();
                else if (m == "CloseMenu") await CerrarAnimacion();
            });
        });
    }

    private async Task AbrirAnimacion()
    {
        if (overlayControl == null || menuControl == null) return;

        // IMPORTANTE: Dejamos de ser transparentes para que los botones funcionen
        this.InputTransparent = false;

        overlayControl.IsVisible = true;
        await Task.WhenAll(
            overlayControl.FadeTo(1, 250),
            menuControl.TranslateTo(0, 0, 400, Easing.CubicOut)
        );
    }

    private async Task CerrarAnimacion()
    {
        if (overlayControl == null || menuControl == null) return;

        await Task.WhenAll(
            overlayControl.FadeTo(0, 250),
            menuControl.TranslateTo(0, 600, 350, Easing.CubicIn)
        );

        overlayControl.IsVisible = false;

        // IMPORTANTE: Volvemos a ser transparentes para liberar los botones de la página (Ajustes)
        this.InputTransparent = true;
    }
}