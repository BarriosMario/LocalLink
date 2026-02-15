using LocalLink.Politicas;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace LocalLink.Login;

public partial class RegisterPage : ContentPage
{
    public class UserData
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }

    public RegisterPage()
	{
		InitializeComponent();
	}

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string username = UsernameEntry.Text?.Trim();
        string password = PassEntry1.Text;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
            return;
        }

        if (email.Contains(" ") || username.Contains(" "))
        {
            await DisplayAlert("Error", "No se permiten espacios", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Correo electrónico inválido", "OK");
            return;
        }

        if (!IsStrongPassword(password))
        {
            await DisplayAlert(
                "Contraseña insegura",
                "La contraseña debe tener mínimo 10 caracteres, incluir una mayúscula, una minúscula, un número y un carácter especial.",
                "OK");

            return;
        }

        // 🔐 Hashear contraseña
        string passwordHash = HashPassword(password);

        UserData user = new UserData
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash
        };

        // Convertir a JSON
        string jsonString = JsonSerializer.Serialize(user);

        //Clave unica y cifrada por modificaciones 
        byte[] key = await GetOrCreateEncryptionKey();
        byte[] encryptedData = EncryptString(jsonString, key);


        // 📁 Ruta segura en dispositivo
        string filePath = Path.Combine(FileSystem.AppDataDirectory, "userData.json");

        File.WriteAllBytes(filePath, encryptedData);

        await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");

        await Navigation.PushAsync(new LocalLink.Login.LoginPage());
    }

    private async Task<byte[]> GetOrCreateEncryptionKey()
    {
        string storedKey = await SecureStorage.Default.GetAsync("encryption_key");

        if (string.IsNullOrEmpty(storedKey))
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();

                string keyString = Convert.ToBase64String(aes.Key);
                await SecureStorage.Default.SetAsync("encryption_key", keyString);

                return aes.Key;
            }
        }

        return Convert.FromBase64String(storedKey);
    }

    private byte[] EncryptString(string plainText, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV(); // IV dinámico

            using (MemoryStream ms = new MemoryStream())
            {
                // Guardamos IV al inicio del archivo
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return ms.ToArray();
            }
        }
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    private bool IsStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10,}$";

        return System.Text.RegularExpressions.Regex.IsMatch(password, pattern);
    }

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        string password = PassEntry1.Text ?? "";

        bool hasLength = password.Length >= 10;
        bool hasLower = System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]");
        bool hasUpper = System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]");
        bool hasNumber = System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
        bool hasSymbol = System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]");

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

        if (!string.IsNullOrEmpty(pass1) &&
            !string.IsNullOrEmpty(pass2) &&
            pass1 == pass2)
        {
            ReqMatch.BackgroundColor = Colors.DarkCyan;
        }
        else
        {
            ReqMatch.BackgroundColor = Colors.White;
        }
    }

    private void UpdateRequirement(BoxView box, bool isValid)
    {
        box.BackgroundColor = isValid ? Colors.DarkCyan : Colors.White;
    }

    private void OnNoSpacesTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            if (entry.Text.Contains(" "))
            {
                entry.Text = entry.Text.Replace(" ", "");
            }
        }
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        var btn = (ImageButton)sender;

        // Si el botón que se presionó es el de la primera contraseña
        if (btn == BtnEye1)
        {
            PassEntry1.IsPassword = !PassEntry1.IsPassword;
            btn.Source = PassEntry1.IsPassword ? "eye_icon.png" : "eye_open_icon.png";
        }
        // Si es el de confirmar contraseña
        else if (btn == BtnEye2)
        {
            PassEntry2.IsPassword = !PassEntry2.IsPassword;
            btn.Source = PassEntry2.IsPassword ? "eye_icon.png" : "eye_open_icon.png";
        }
    }

    private async void OpenTermsAndConditions(object sender, EventArgs e)
    {
        // Navega a la vista de Términos
        await Navigation.PushAsync(new TermsAndConditions());
    }

    private async void OpenPrivacyPolicy(object sender, EventArgs e)
    {
        // Navegamos a la vista de Política de Privacidad
        // Asegúrate de que el Namespace coincida con donde creaste la página
        await Navigation.PushAsync(new LocalLink.Politicas.PrivacyPolicy());
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Regresa exactamente a la pantalla de donde vino el usuario
        await Navigation.PopAsync();
    }
}