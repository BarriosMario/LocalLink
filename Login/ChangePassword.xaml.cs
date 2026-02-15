using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LocalLink.Login;

public partial class ChangePassword : ContentPage
{
    private string userEmail;

    public ChangePassword(string email)
    {
        InitializeComponent();
        userEmail = email;
    }

    // =========================
    // VALIDACIÓN EN TIEMPO REAL
    // =========================
    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        string password = PassEntry1.Text ?? "";
        string confirm = PassEntry2.Text ?? "";

        bool hasLength = password.Length >= 10;
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasNumber = Regex.IsMatch(password, "[0-9]");
        bool hasSymbol = Regex.IsMatch(password, "[^a-zA-Z0-9]");
        bool match = password == confirm && password.Length > 0;

        ReqLength.BackgroundColor = hasLength ? Colors.DarkCyan : Colors.White;
        ReqLower.BackgroundColor = hasLower ? Colors.DarkCyan : Colors.White;
        ReqUpper.BackgroundColor = hasUpper ? Colors.DarkCyan : Colors.White;
        ReqNumber.BackgroundColor = hasNumber ? Colors.DarkCyan : Colors.White;
        ReqSymbol.BackgroundColor = hasSymbol ? Colors.DarkCyan : Colors.White;
        ReqMatch.BackgroundColor = match ? Colors.DarkCyan : Colors.White;
    }

    // =========================
    // MOSTRAR / OCULTAR PASSWORD
    // =========================
    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        if (sender == BtnEye1)
            PassEntry1.IsPassword = !PassEntry1.IsPassword;

        if (sender == BtnEye2)
            PassEntry2.IsPassword = !PassEntry2.IsPassword;
    }

    // =========================
    // BOTÓN ACTUALIZAR
    // =========================
    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        string password = PassEntry1.Text ?? "";
        string confirm = PassEntry2.Text ?? "";

        // 🔎 Validación lógica real (no depende del color)
        bool hasLength = password.Length >= 10;
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasNumber = Regex.IsMatch(password, "[0-9]");
        bool hasSymbol = Regex.IsMatch(password, "[^a-zA-Z0-9]");
        bool match = password == confirm;

        if (!(hasLength && hasLower && hasUpper && hasNumber && hasSymbol && match))
        {
            await DisplayAlert("Error", "La contraseña no cumple los requisitos.", "OK");
            return;
        }

        await UpdatePassword(password);

    }

    // =========================
    // ACTUALIZAR CONTRASEÑA
    // =========================
    private async Task UpdatePassword(string newPassword)
    {
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

            byte[] decryptedBytes = DecryptBytes(encryptedData, key);
            string jsonString = Encoding.UTF8.GetString(decryptedBytes);

            var storedUser = JsonSerializer.Deserialize<RegisterPage.UserData>(jsonString);

            if (storedUser == null || storedUser.Email != userEmail)
            {
                await DisplayAlert("Error", "Cuenta no encontrada", "OK");
                return;
            }

            // 🔐 Actualizar contraseña hasheada
            string newHashedPassword = HashPassword(newPassword);

            // 🚫 Verificar que no sea igual a la anterior
            if (storedUser.PasswordHash == newHashedPassword)
            {
                await DisplayAlert("Error", "La nueva contraseña no puede ser igual a la anterior.", "OK");
                return;
            }

            // 🔐 Actualizar contraseña
            storedUser.PasswordHash = newHashedPassword;


            string updatedJson = JsonSerializer.Serialize(storedUser);
            byte[] newEncryptedData = EncryptString(updatedJson, key);

            File.WriteAllBytes(filePath, newEncryptedData);

            await DisplayAlert("Éxito", "Contraseña actualizada correctamente", "OK");

            await Navigation.PopToRootAsync();
        }
        catch
        {
            await DisplayAlert("Error", "Error al actualizar contraseña", "OK");
        }
    }

    // =========================
    // OBTENER CLAVE DE CIFRADO
    // =========================
    private async Task<byte[]> GetEncryptionKey()
    {
        string storedKey = await SecureStorage.Default.GetAsync("encryption_key");

        if (string.IsNullOrEmpty(storedKey))
            throw new Exception("Clave de cifrado no encontrada");

        return Convert.FromBase64String(storedKey);
    }

    // =========================
    // DESENCRIPTAR
    // =========================
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

    // =========================
    // ENCRIPTAR
    // =========================
    private byte[] EncryptString(string plainText, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();

            using (MemoryStream ms = new MemoryStream())
            {
                // Guardar IV al inicio
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

    // =========================
    // HASH CONTRASEÑA
    // =========================
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }
}
