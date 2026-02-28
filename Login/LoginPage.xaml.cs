using System;
using LocalLink.Repositories;
using LocalLink.Services;
using LocalLink.Models;
using Microsoft.Maui.Controls;
using System.Linq;
using System.IO;

namespace LocalLink.Login;

public partial class LoginPage : ContentPage
{
    private readonly UserRepository _userRepository = new UserRepository();
    private readonly PasswordService _passwordService = new PasswordService();

    private int failedAttempts = 0;
    private const int MaxAttempts = 5;

    public LoginPage()
    {
        InitializeComponent();
        MostrarRutaJson();
    }

    private async void MostrarRutaJson()
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, "userData.json");
        await DisplayAlert("Ruta del archivo JSON", filePath, "OK");
    }

    // NUEVO: Lógica para alternar el tema
    private void OnThemeToggleButtonClicked(object sender, EventArgs e)
    {
        if (Application.Current.UserAppTheme == AppTheme.Light)
            Application.Current.UserAppTheme = AppTheme.Dark;
        else
            Application.Current.UserAppTheme = AppTheme.Light;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Por favor ingresa tu correo y contraseña.", "OK");
            return;
        }

        var users = await _userRepository.GetAllUsersAsync();
        var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            await DisplayAlert("Error", "Correo electrónico no registrado.", "OK");
            return;
        }

        bool isPasswordCorrect = _passwordService.VerifyPassword(password, user.PasswordHash);
        if (isPasswordCorrect)
        {
            failedAttempts = 0;
            // Nota: Aquí puedes reinsertar el DeviceAccountManager si lo necesitas
            Preferences.Default.Set("is_registered", true);
            Preferences.Default.Set("current_user_email", user.Email);
            await DisplayAlert("Éxito", $"Bienvenido {user.Username}", "OK");
            Application.Current.MainPage = new AppShell();
            return;
        }

        failedAttempts++;
        if (failedAttempts == 3)
        {
            bool recover = await DisplayAlert("Contraseña incorrecta", "¿Olvidaste tu contraseña?", "Sí", "No");
            if (recover)
            {
                await Navigation.PushAsync(new LocalLink.Login.EmailVerification());
                return;
            }
            else
            {
                int remaining = MaxAttempts - failedAttempts;
                await DisplayAlert("Atención", $"Solo te quedan {remaining} intentos de contraseña.", "OK");
                return;
            }
        }
        else if (failedAttempts >= MaxAttempts)
        {
            await DisplayAlert("Cuenta bloqueada", "Has superado el número máximo de intentos. Tu cuenta ha sido bloqueada temporalmente.", "OK");
            return;
        }
        else
        {
            int remaining = MaxAttempts - failedAttempts;
            await DisplayAlert("Error", $"Contraseña incorrecta. Te quedan {remaining} intentos.", "OK");
        }
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        var btn = (ImageButton)sender;
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        btn.Source = PasswordEntry.IsPassword ? "eye_off_icon.png" : "eye_on_icon.png";
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocalLink.Login.EmailVerification());
    }
}