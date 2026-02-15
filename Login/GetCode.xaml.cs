using System;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using System.Linq;

namespace LocalLink.Login;

public partial class GetCode : ContentPage
{
    private string generatedCode;
    private string userEmail;
    private bool isFilling = false;


    public GetCode(string email)
    {
        InitializeComponent();
        userEmail = email;

        LblUserEmail.Text = MaskEmail(email);

        GenerateAndShowCode();
    }



    private async void GenerateAndShowCode()
    {
        Random random = new Random();
        generatedCode = random.Next(100000, 999999).ToString();

        // Copiar al portapapeles
        await Clipboard.SetTextAsync(generatedCode);

        await DisplayAlert(
            "Código de verificación",
            $"Tu código es: {generatedCode}\n\n(Se copió automáticamente al portapapeles)",
            "OK");
    }

    private void OnDigitTextChanged(object sender, TextChangedEventArgs e)
    {
        if (isFilling)
            return;

        if (sender is not Entry currentEntry)
            return;

        string value = e.NewTextValue ?? "";

        if (string.IsNullOrEmpty(value))
            return;

        // 🔹 Si se pegaron múltiples caracteres (desde cualquier entry)
        if (value.Length > 1)
        {
            FillDigits(value);
            return;
        }

        // 🔹 Solo permitir números
        if (!char.IsDigit(value[0]))
        {
            currentEntry.Text = "";
            return;
        }

        MoveToNext(currentEntry);
    }

    private void FillDigits(string pastedText)
    {
        isFilling = true;

        var digits = pastedText.Where(char.IsDigit).Take(6).ToArray();

        Digit1.Text = digits.ElementAtOrDefault(0).ToString();
        Digit2.Text = digits.ElementAtOrDefault(1).ToString();
        Digit3.Text = digits.ElementAtOrDefault(2).ToString();
        Digit4.Text = digits.ElementAtOrDefault(3).ToString();
        Digit5.Text = digits.ElementAtOrDefault(4).ToString();
        Digit6.Text = digits.ElementAtOrDefault(5).ToString();

        isFilling = false;

        Digit6.Focus();
    }


    private void MoveToNext(Entry current)
    {
        if (current == Digit1) Digit2.Focus();
        else if (current == Digit2) Digit3.Focus();
        else if (current == Digit3) Digit4.Focus();
        else if (current == Digit4) Digit5.Focus();
        else if (current == Digit5) Digit6.Focus();
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        string enteredCode =
            (Digit1.Text ?? "") +
            (Digit2.Text ?? "") +
            (Digit3.Text ?? "") +
            (Digit4.Text ?? "") +
            (Digit5.Text ?? "") +
            (Digit6.Text ?? "");

        if (enteredCode == generatedCode)
        {
            await DisplayAlert("Correcto", "Código verificado", "OK");

            await Navigation.PushAsync(new LocalLink.Login.ChangePassword(userEmail));
        }
        else
        {
            await DisplayAlert("Error", "Código incorrecto", "OK");
        }
    }
    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            return email;

        var parts = email.Split('@');
        string namePart = parts[0];
        string domainPart = parts[1];

        if (namePart.Length <= 4)
            return email; // demasiado corto para ocultar

        string firstTwo = namePart.Substring(0, 2);
        string lastTwo = namePart.Substring(namePart.Length - 2);

        string masked = firstTwo +
                        new string('*', namePart.Length - 4) +
                        lastTwo;

        return $"{masked}@{domainPart}";
    }

    private async void OnNotYourAccountClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); // Esto lo regresa a EmailVerification
    }
}