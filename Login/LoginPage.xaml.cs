using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace LocalLink.Login;

public partial class LoginPage : ContentPage
{
    private int failedAttempts = 0;

    public LoginPage()
	{
		InitializeComponent();
	}

    private byte[] DecryptBytes(byte[] encryptedData, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            using (MemoryStream ms = new MemoryStream(encryptedData))
            {
                byte[] iv = new byte[16];
                ms.Read(iv, 0, iv.Length);
                aes.IV = iv;

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (MemoryStream resultStream = new MemoryStream())
                {
                    cs.CopyTo(resultStream);
                    return resultStream.ToArray();
                }
            }
        }
    }

    private async Task<byte[]> GetEncryptionKey()
    {
        string storedKey = await SecureStorage.Default.GetAsync("encryption_key");

        if (string.IsNullOrEmpty(storedKey))
            return null;

        return Convert.FromBase64String(storedKey);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Completa todos los campos", "OK");
            return;
        }

        string filePath = Path.Combine(FileSystem.AppDataDirectory, "userData.json");

        if (!File.Exists(filePath))
        {
            await DisplayAlert("Error", "Cuenta no encontrada", "OK");
            return;
        }

        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] key = await GetEncryptionKey();

            if (key == null)
            {
                await DisplayAlert("Error", "Error de seguridad", "OK");
                return;
            }

            byte[] decryptedBytes = DecryptBytes(encryptedData, key);
            string jsonString = Encoding.UTF8.GetString(decryptedBytes);

            var storedUser = JsonSerializer.Deserialize<RegisterPage.UserData>(jsonString);

            if (storedUser == null || storedUser.Email != email)
            {
                await DisplayAlert("Error", "Cuenta no encontrada", "OK");
                return;
            }

            string enteredHash = HashPassword(password);

            if (storedUser.PasswordHash != enteredHash)
            {
                failedAttempts++;

                if (failedAttempts >= 3)
                {
                    bool reset = await DisplayAlert(
                        "Olvidaste tu contraseña",
                        "Has intentado 3 veces. ¿Deseas recuperarla?",
                        "Sí",
                        "No");

                    if (reset)
                    {
                        await Navigation.PushAsync(new EmailVerification());
                    }

                    failedAttempts = 0;
                    return;
                }

                await DisplayAlert("Error", "Contraseña incorrecta", "OK");
                return;
            }

            // ✅ LOGIN EXITOSO
            failedAttempts = 0;

            await DisplayAlert("Bienvenido", "Inicio de sesión exitoso", "OK");

            await Navigation.PushAsync(new MainPage());
        }
        catch
        {
            await DisplayAlert("Error", "Error al leer datos", "OK");
        }

        await Navigation.PushAsync(new LocalLink.MainPage());
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


    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        // 1. Invertimos el estado de la contraseña
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

        // 2. Obtenemos el botón que disparó el evento
        var btn = (ImageButton)sender;

        // 3. Cambiamos la imagen según el estado de IsPassword
        // Si IsPassword es true (está oculto), mostramos el icono para "abrir" el ojo
        // Si IsPassword es false (se ve), mostramos el icono del ojo "tachado" o abierto
        btn.Source = PasswordEntry.IsPassword ? "eye_icon.png" : "eye_open_icon.png";
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LocalLink.Login.EmailVerification());
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Si usas NavigationPage tradicional:
        await Navigation.PushAsync(new LocalLink.Login.RegisterPage());
    }
}