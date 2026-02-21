using LocalLink.Models;
using LocalLink.Services;
using LocalLink.Repositories;
using System.Text.RegularExpressions;

namespace LocalLink.Login;

public partial class RegisterPage : ContentPage
{
    private readonly UserRepository _userRepository = new UserRepository();
    private readonly PasswordService _passwordService = new PasswordService();

    public RegisterPage()
    {
        InitializeComponent();
    }

    // ✅ Registro de usuario
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string username = UsernameEntry.Text?.Trim();
        string password = PassEntry1.Text;
        string passwordConfirm = PassEntry2.Text;

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Correo electrónico inválido", "OK");
            return;
        }

        string passwordError = _passwordService.GetPasswordRequirementsMessage(password);
        if (passwordError != null)
        {
            await DisplayAlert("Contraseña insegura", passwordError, "OK");
            return;
        }


        if (password != passwordConfirm)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            return;
        }

        //Terminos y condiciones
        if (!TermsCheckBox.IsChecked)
        {
            await DisplayAlert("Error", "Para continuar, es necesario que acepte los Términos y Condiciones y la Política de Privacidad.", "OK");
            return;
        }

        // Validar si email o username ya existen
        if (await _userRepository.EmailExistsAsync(email))
        {
            await DisplayAlert("Error", "Este correo ya está registrado", "OK");
            return;
        }

        if (await _userRepository.UsernameExistsAsync(username))
        {
            await DisplayAlert("Error", "Este nombre de usuario ya existe", "OK");
            return;
        }

        // Crear usuario con hash seguro
        string passwordHash = _passwordService.HashPassword(password);

        var newUser = new UserData
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash
        };

        await _userRepository.AddUserAsync(newUser);

        // Mostrar alerta con el username
        await DisplayAlert("¡Éxito!", $"Usuario \"{username}\" registrado correctamente", "OK");

        // Navegar a LoginPage
        await Navigation.PushAsync(new LoginPage());
    }

    // Validación de email básica
    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    // Otras funciones auxiliares de tu XAML

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        string password = PassEntry1.Text ?? "";

        bool hasLength = password.Length >= 10;
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasNumber = Regex.IsMatch(password, @"\d");
        bool hasSymbol = Regex.IsMatch(password, @"[\W_]");

        UpdateRequirement(ReqLength, hasLength);
        UpdateRequirement(ReqLower, hasLower);
        UpdateRequirement(ReqUpper, hasUpper);
        UpdateRequirement(ReqNumber, hasNumber);
        UpdateRequirement(ReqSymbol, hasSymbol);

        ValidatePasswordMatch();
    }

    private void ValidatePasswordMatch()
    {
        string pass1 = PassEntry1.Text ?? "";
        string pass2 = PassEntry2.Text ?? "";

        ReqMatch.BackgroundColor = (pass1 == pass2 && !string.IsNullOrEmpty(pass1)) ? Colors.DarkCyan : Colors.White;
    }

    private void UpdateRequirement(BoxView box, bool isValid)
    {
        box.BackgroundColor = isValid ? Colors.DarkCyan : Colors.White;
    }

    private void OnNoSpacesTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.Text.Contains(" "))
        {
            entry.Text = entry.Text.Replace(" ", "");
        }
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        var btn = (ImageButton)sender;

        if (btn == BtnEye1)
        {
            PassEntry1.IsPassword = !PassEntry1.IsPassword;
            btn.Source = PassEntry1.IsPassword ? "eye_off_icon.png" : "eye_on_icon.png";
        }
        else if (btn == BtnEye2)
        {
            PassEntry2.IsPassword = !PassEntry2.IsPassword;
            btn.Source = PassEntry2.IsPassword ? "eye_off_icon.png" : "eye_on_icon.png";
        }
    }

    private async void OpenTermsAndConditions(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocalLink.Politicas.TermsAndConditions());
    }

    private async void OpenPrivacyPolicy(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocalLink.Politicas.PrivacyPolicy());
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
