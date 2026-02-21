using LocalLink.Models;
using LocalLink.Repositories;
using LocalLink.Services;
using Microsoft.Maui.Controls.Shapes;

namespace LocalLink.Vistas;

public partial class AcountPage : ContentPage
{
    private readonly UserRepository _userRepository;

    public AcountPage(UserRepository userRepository)
    {
        InitializeComponent();
        _userRepository = userRepository;

        LoadCurrentUser();
    }

    private async void LoadCurrentUser()
    {
        string currentEmail = Preferences.Get("current_user_email", string.Empty);

        if (string.IsNullOrEmpty(currentEmail))
        {
            UsernameLabel.Text = "INVITADO";
            HeaderProfileImage.Source = "default_avatar.png"; // 🔹 Mostrar default
            return;
        }

        var users = await _userRepository.GetAllUsersAsync();

        var user = users.FirstOrDefault(u =>
            u.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase));

        UsernameLabel.Text = user?.Username?.ToUpper() ?? "USUARIO";

        // 🔹 Mostrar imagen del usuario o default
        HeaderProfileImage.Source = string.IsNullOrEmpty(user?.ProfileImagePath)
            ? "default_avatar.png"
            : user.ProfileImagePath;
    }

    private async void LoadProfiles()
    {
        ProfilesList.Children.Clear();

        var users = await _userRepository.GetAllUsersAsync();
        var activeProfiles = DeviceAccountManager.GetAccounts();
        string currentEmail = Preferences.Get("current_user_email", string.Empty);

        // 🔹 Reordenar: poner primero el usuario activo
        var sortedEmails = activeProfiles
            .OrderByDescending(email => email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var email in sortedEmails)
        {
            var user = users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null) continue;

            bool isActive = email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase);

            var grid = new Grid
            {
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        },
                Padding = new Thickness(0, 5)
            };

            // 🔹 BOTÓN PRINCIPAL (Imagen + Nombre + Estado)
            var mainButton = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Colors.Transparent,
                Padding = 0
            };

            var contentGrid = new Grid
            {
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = 50 },
            new ColumnDefinition { Width = GridLength.Star }
        }
            };

            var imageFrame = new Frame
            {
                WidthRequest = 40,
                HeightRequest = 40,
                CornerRadius = 20,
                Padding = 2,
                IsClippedToBounds = true,
                HasShadow = false,
                BorderColor = isActive ? Colors.DarkCyan : Colors.LightGray,
                BackgroundColor = Colors.Transparent,
                Content = new Image
                {
                    Aspect = Aspect.AspectFill,
                    Source = string.IsNullOrEmpty(user.ProfileImagePath)
                        ? "default_avatar.png"
                        : user.ProfileImagePath
                }
            };

            var nameStack = new StackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center
            };

            var nameLabel = new Label
            {
                Text = user.Username?.ToUpper() ?? "USUARIO",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black
            };

            var statusLabel = new Label
            {
                Text = isActive ? "Activo" : "Inactivo",
                FontSize = 12,
                TextColor = isActive ? Colors.DarkCyan : Colors.Gray
            };

            nameStack.Children.Add(nameLabel);
            nameStack.Children.Add(statusLabel);

            contentGrid.Add(imageFrame);
            Grid.SetColumn(imageFrame, 0);

            contentGrid.Add(nameStack);
            Grid.SetColumn(nameStack, 1);

            mainButton.Content = contentGrid;

            // 🔥 Tap gesture (equivalente a botón)
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                Preferences.Set("current_user_email", user.Email);
                Overlay.IsVisible = false;
                Shell.SetTabBarIsVisible(this, true);
                LoadCurrentUser();
                LoadProfiles();
            };

            mainButton.GestureRecognizers.Add(tapGesture);

            // 🔹 BOTÓN VER
            var btnView = new Button
            {
                Text = "Ver",
                BackgroundColor = Colors.DarkCyan,
                TextColor = Colors.White,
                CornerRadius = 10,
                HeightRequest = 35,
                WidthRequest = 70
            };

            btnView.Clicked += async (s, e) =>
            {
                // Cerrar overlay
                Overlay.IsVisible = false;
                Shell.SetTabBarIsVisible(this, true);

                // Navegar a detalles del perfil seleccionado
                await Navigation.PushAsync(new Profile.ProfileDetailsPage(_userRepository, user.Email));
            };

            grid.Add(mainButton);
            Grid.SetColumn(mainButton, 0);

            grid.Add(btnView);
            Grid.SetColumn(btnView, 1);

            ProfilesList.Children.Add(grid);
        }
    }

    private async void OnOpenMenu(object sender, EventArgs e)
    {
        Overlay.IsVisible = true;
        BottomMenu.TranslationY = BottomMenu.Height;
        Shell.SetTabBarIsVisible(this, false);
        await BottomMenu.TranslateTo(0, 0, 250, Easing.CubicOut);
        LoadProfiles();
    }

    private async void OnCloseMenu(object sender, EventArgs e)
    {
        await BottomMenu.TranslateTo(0, BottomMenu.Height, 200, Easing.CubicIn);
        Overlay.IsVisible = false;
        Shell.SetTabBarIsVisible(this, true);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        string currentEmail = Preferences.Get("current_user_email", "");

        bool confirm = await DisplayAlert(
            "Cerrar sesión",
            "¿Deseas cerrar sesión?",
            "Sí",
            "Cancelar");

        if (!confirm)
            return;

        Overlay.IsVisible = false;

        // 🔹 Eliminar cuenta actual del dispositivo
        DeviceAccountManager.RemoveAccount(currentEmail);

        var remainingAccounts = DeviceAccountManager.GetAccounts();

        if (remainingAccounts.Count == 0)
        {
            Preferences.Set("is_registered", false);
            Preferences.Remove("current_user_email");

            Application.Current.MainPage =
                new NavigationPage(new Login.LoginPage());

            return;
        }

        // 🔹 Cambiar a otra cuenta activa
        string newAccount = remainingAccounts.First();
        Preferences.Set("current_user_email", newAccount);

        await DisplayAlert(
            "Cuenta cambiada",
            $"Se cerró sesión de:\n{currentEmail}\n\nAhora estás en:\n{newAccount}",
            "OK");

        LoadCurrentUser();
        LoadProfiles();
    }

    private async void OnAddProfileClicked(object sender, EventArgs e)
    {
        var accounts = DeviceAccountManager.GetAccounts();

        if (accounts.Count >= 3)
        {
            await DisplayAlert(
                "Límite de cuentas alcanzado",
                "Si desea añadir otra, cierre sesión de una cuenta e inicie sesión en la nueva cuenta.",
                "Entendido");
            return;
        }

        try
        {
            // 🔹 Navegar usando Shell, TabBar se oculta si LoginPage tiene Shell.TabBarIsVisible="False"
            await Shell.Current.GoToAsync(nameof(Login.LoginPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo navegar: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteProfileClicked(object sender, EventArgs e)
    {
        string currentEmail = Preferences.Get("current_user_email", "");

        bool confirm = await DisplayAlert(
            "Eliminar cuenta",
            $"¿Estás seguro de eliminar la cuenta\n{currentEmail}\nde manera permanente?",
            "Sí",
            "No");

        if (!confirm)
            return;

        Overlay.IsVisible = false;

        // 🔴 Eliminar datos permanentes
        await _userRepository.DeleteUserAsync(currentEmail);

        // 🔴 Eliminar del dispositivo
        DeviceAccountManager.RemoveAccount(currentEmail);

        var remainingAccounts = DeviceAccountManager.GetAccounts();

        if (remainingAccounts.Count == 0)
        {
            Preferences.Set("is_registered", false);
            Preferences.Remove("current_user_email");

            await DisplayAlert(
                "Cuenta eliminada",
                "La cuenta fue eliminada permanentemente.",
                "OK");

            Application.Current.MainPage =
                new NavigationPage(new Login.LoginPage());

            return;
        }

        // 🔹 Cambiar a otra cuenta activa
        string newAccount = remainingAccounts.First();
        Preferences.Set("current_user_email", newAccount);

        await DisplayAlert(
            "Cuenta eliminada",
            $"Se eliminó permanentemente:\n{currentEmail}\n\nAhora estás en:\n{newAccount}",
            "OK");

        LoadCurrentUser();
        LoadProfiles();
    }
}