using System;
using LocalLink.Repositories;
using LocalLink.Services; // PasswordService
using LocalLink.Models;  // UserData
using Microsoft.Maui.Controls;

namespace LocalLink.Login;

public partial class LoginPage : ContentPage
{
    private readonly UserRepository _userRepository = new UserRepository();
    private readonly PasswordService _passwordService = new PasswordService();

    private int failedAttempts = 0; // cuenta de intentos fallidos
    private const int MaxAttempts = 5;

    private async void MostrarRutaJson()
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, "userData.json");

        await DisplayAlert("Ruta del archivo JSON", filePath, "OK");
    }

    public LoginPage()
    {
        InitializeComponent();
        MostrarRutaJson();
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

        // 1️⃣ Verificar si el email existe
        var users = await _userRepository.GetAllUsersAsync();

        var user = users
            .FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            await DisplayAlert("Error", "Correo electrónico no registrado.", "OK");
            return;
        }

        // 3️⃣ Verificar contraseña
        bool isPasswordCorrect = _passwordService.VerifyPassword(password, user.PasswordHash);
        if (isPasswordCorrect)
        {
            failedAttempts = 0;

            bool canAdd = DeviceAccountManager.TryAddAccount(user.Email);
            if (!canAdd)
            {
                await DisplayAlert(
                    "Límite alcanzado",
                    "No puedes tener más de 3 cuentas en este dispositivo.",
                    "OK");
                return;
            }

            Preferences.Default.Set("is_registered", true);
            Preferences.Default.Set("current_user_email", user.Email);

            await DisplayAlert("Éxito", $"Bienvenido {user.Username}", "OK");

            Application.Current.MainPage = new AppShell();
            return;
        }


        // 4️⃣ Contraseña incorrecta
        failedAttempts++;

        if (failedAttempts == 3)
        {
            bool recover = await DisplayAlert(
                "Contraseña incorrecta",
                "Ha fallado 3 veces. ¿Olvidaste tu contraseña?",
                "Sí",
                "No");

            if (recover)
            {
                // Redirigir a EmailVerification
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
            // Opcional: bloquear el usuario en un campo isBlocked en UserRepository
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
        // Redirige a email verification para cambiar contraseña
        await Navigation.PushAsync(new LocalLink.Login.EmailVerification());
    }
}
