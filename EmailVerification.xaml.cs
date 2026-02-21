using System;
using System.Linq;
using LocalLink.Repositories;
using LocalLink.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LocalLink.Login;

public partial class EmailVerification : ContentPage
{
    private readonly UserRepository _userRepository = new UserRepository();
    private string _verificationCode;
    private string _userEmail;

    public EmailVerification()
    {
        InitializeComponent();
    }

    private async void OnSendCodeClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Error", "Por favor ingresa tu correo electrónico.", "OK");
            return;
        }

        bool exists = await _userRepository.EmailExistsAsync(email);
        if (!exists)
        {
            await DisplayAlert("Error", "El correo electrónico no está registrado.", "OK");
            return;
        }

        // Guardamos el correo para la siguiente página
        _userEmail = email;

        // Generamos código de verificación aleatorio de 6 dígitos
        Random rnd = new Random();
        _verificationCode = rnd.Next(100000, 999999).ToString();

        // Copiar al portapapeles
        await Clipboard.Default.SetTextAsync(_verificationCode);

        // Redirigir a GetCode, pasando el correo y código como parámetros
        await Navigation.PushAsync(new LocalLink.Login.GetCode(_userEmail));
    }

    private async void OnSuggestionTapped(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            EmailEntry.Text = label.Text;
            SuggestionsBorder.IsVisible = false;
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
