using Microsoft.Maui.Controls;
using LocalLink.Repositories;
using LocalLink.Services;
using LocalLink.Models; 


namespace LocalLink.Login;

public partial class ChangePassword : ContentPage
{
    private readonly UserRepository _userRepository = new();
    private readonly PasswordService _passwordService = new();
    private string _userEmail;
    private UserData _currentUser; // <-- de LocalLink.Models

    public ChangePassword(string email)
    {
        InitializeComponent();
        _userEmail = email;
        EmailLabel.Text = email;
        LoadCurrentUser();
    }

    private async void LoadCurrentUser()
    {
        var users = await _userRepository.GetAllUsersAsync();
        _currentUser = users.FirstOrDefault(u => u.Email.Equals(_userEmail, StringComparison.OrdinalIgnoreCase));
        if (_currentUser == null)
        {
            await DisplayAlert("Error", "Usuario no encontrado.", "OK");
            await Navigation.PopAsync();
        }
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button)
        {
            if (button == BtnEye1)
            {
                PassEntry1.IsPassword = !PassEntry1.IsPassword;
                BtnEye1.Source = PassEntry1.IsPassword ? "eye_off_icon.png" : "eye_on_icon.png";
            }
            else if (button == BtnEye2)
            {
                PassEntry2.IsPassword = !PassEntry2.IsPassword;
                BtnEye2.Source = PassEntry2.IsPassword ? "eye_off_icon.png" : "eye_on_icon.png";
            }
        }
    }

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        string password = PassEntry1.Text ?? "";
        string confirm = PassEntry2.Text ?? "";

        // Validaciones individuales
        ReqLength.BackgroundColor = password.Length >= 10 ? Colors.DarkCyan : Colors.White;
        ReqLower.BackgroundColor = password.Any(char.IsLower) ? Colors.DarkCyan : Colors.White;
        ReqUpper.BackgroundColor = password.Any(char.IsUpper) ? Colors.DarkCyan : Colors.White;
        ReqNumber.BackgroundColor = password.Any(char.IsDigit) ? Colors.DarkCyan : Colors.White;
        ReqSymbol.BackgroundColor = password.Any(ch => !char.IsLetterOrDigit(ch)) ? Colors.DarkCyan : Colors.White;

        // Coincidencia
        ReqMatch.BackgroundColor =
            (!string.IsNullOrEmpty(password) && password == confirm)
            ? Colors.DarkCyan
            : Colors.White;
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        string newPassword = PassEntry1.Text?.Trim();
        string confirmPassword = PassEntry2.Text?.Trim();

        if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            await DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
            return;
        }

        // Validar que coincidan
        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        // Validar fuerza de la contraseña
        if (!_passwordService.IsStrongPassword(newPassword))
        {
            string msg = _passwordService.GetPasswordRequirementsMessage(newPassword);
            await DisplayAlert("Error", msg, "OK");
            return;
        }

        // Validar que no sea igual a la contraseña anterior
        if (_passwordService.VerifyPassword(newPassword, _currentUser.PasswordHash))
        {
            await DisplayAlert("Error", "La nueva contraseña no puede ser igual a la anterior.", "OK");
            return;
        }

        // Hashear y guardar
        _currentUser.PasswordHash = _passwordService.HashPassword(newPassword);
        await _userRepository.UpdateUserAsync(_currentUser);

        await DisplayAlert("Éxito", $"Contraseña de {_currentUser.Username} cambiada exitosamente.", "OK");

        // Redirigir al login
        await Navigation.PopToRootAsync(); // Asumiendo que LoginPage es la raíz
    }
}
