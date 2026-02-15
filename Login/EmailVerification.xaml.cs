using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace LocalLink.Login;

public partial class EmailVerification : ContentPage
{
    private readonly List<string> _domains = new()
    {
        "@gmail.com", "@outlook.com", "@hotmail.com", "@yahoo.com", "@icloud.com"
    };

    public EmailVerification()
	{
		InitializeComponent();
	}

    private async Task<byte[]> GetEncryptionKey()
    {
        string storedKey = await SecureStorage.Default.GetAsync("encryption_key");

        if (string.IsNullOrEmpty(storedKey))
            return null;

        return Convert.FromBase64String(storedKey);
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


    private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue;

        // Si el usuario escribe '@', mostramos sugerencias
        if (!string.IsNullOrEmpty(texto) && texto.Contains("@"))
        {
            string nombreUsuario = texto.Split('@')[0];

            // Creamos las sugerencias combinando lo que escribió con los dominios
            var sugerencias = _domains.Select(d => nombreUsuario + d).ToList();

            SuggestionsList.ItemsSource = sugerencias;
            SuggestionsBorder.IsVisible = true;
        }
        else
        {
            SuggestionsBorder.IsVisible = false;
        }
    }

    private void OnSuggestionTapped(object sender, EventArgs e)
    {
        var label = sender as Label;
        if (label != null)
        {
            // Al tocar una sugerencia, la ponemos en el Entry y ocultamos la lista
            EmailEntry.Text = label.Text;
            SuggestionsBorder.IsVisible = false;
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        // Regresa exactamente a la pantalla de donde vino el usuario
        await Navigation.PopAsync();
    }

    private async void OnSendCodeClicked(object sender, EventArgs e)
    {
        string emailIngresado = EmailEntry.Text?.Trim();

        if (!IsEmailValid(emailIngresado))
        {
            await DisplayAlert("Error", "Por favor ingresa un correo válido", "OK");
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

            if (storedUser == null || storedUser.Email != emailIngresado)
            {
                await DisplayAlert("Error", "Cuenta no encontrada", "OK");
                return;
            }

            // ✅ Si existe → pasamos sin mostrar alerta
            await Navigation.PushAsync(new GetCode(emailIngresado));
        }
        catch
        {
            await DisplayAlert("Error", "Error al verificar la cuenta", "OK");
        }
    }


    public bool IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        // Esta expresión regular valida que tenga algo@dominio.extension
        // Acepta .com, .com.mx, .edu, .net, etc.
        string regex = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,6}$";

        return Regex.IsMatch(email, regex);
    }
}