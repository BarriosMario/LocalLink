using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace LocalLink.Login;

public partial class GetCode : ContentPage
{
    private string generatedCode;
    private string userEmail;
    private bool isFilling = false;
    private int failedAttempts = 0; // Contador de intentos

    private bool hasShownAlert = false;

    public GetCode(string email)
    {
        InitializeComponent();
        userEmail = email;

        LblUserEmail.Text = MaskEmail(email);

        GenerateAndShowCode();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Solo mostrar el alert una vez
        if (!hasShownAlert)
        {
            hasShownAlert = true;
            GenerateAndShowCode();
        }
    }

    private async void GenerateAndShowCode()
    {
        Random random = new Random();
        generatedCode = random.Next(100000, 999999).ToString();

        // Copiar al portapapeles automáticamente
        await Clipboard.SetTextAsync(generatedCode);

        await DisplayAlert(
            "Código de verificación",
            $"Tu código es: {generatedCode}\n\n(Se copió automáticamente al portapapeles)",
            "OK");
    }

    private void OnDigitTextChanged(object sender, TextChangedEventArgs e)
    {
        if (isFilling || sender is not Entry currentEntry)
            return;

        string newVal = e.NewTextValue ?? "";

        // Validar solo números
        if (!string.IsNullOrEmpty(newVal) && !char.IsDigit(newVal[^1]))
        {
            currentEntry.Text = ""; // Limpiar si no es número
            return;
        }

        // Si se pegó un texto completo
        if (newVal.Length > 1)
        {
            FillDigits(newVal);
            return;
        }

        if (!string.IsNullOrEmpty(newVal))
        {
            MoveToNext(currentEntry);
        }
        else
        {
            MoveToPrevious(currentEntry);
        }
    }

    private void FillDigits(string pastedText)
    {
        if (string.IsNullOrWhiteSpace(pastedText)) return;

        isFilling = true;

        var digits = pastedText.Where(char.IsDigit).Take(6).ToList();

        if (digits.Count > 0) Digit1.Text = digits[0].ToString();
        if (digits.Count > 1) Digit2.Text = digits[1].ToString();
        if (digits.Count > 2) Digit3.Text = digits[2].ToString();
        if (digits.Count > 3) Digit4.Text = digits[3].ToString();
        if (digits.Count > 4) Digit5.Text = digits[4].ToString();
        if (digits.Count > 5) Digit6.Text = digits[5].ToString();

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

    private void MoveToPrevious(Entry current)
    {
        if (current == Digit6) Digit5.Focus();
        else if (current == Digit5) Digit4.Focus();
        else if (current == Digit4) Digit3.Focus();
        else if (current == Digit3) Digit2.Focus();
        else if (current == Digit2) Digit1.Focus();
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
            failedAttempts++;

            if (failedAttempts == 3)
            {
                bool recover = await DisplayAlert(
                    "Código incorrecto",
                    "Has fallado 3 veces. ¿Deseas recuperar tu contraseña?",
                    "Sí",
                    "No");

                if (recover)
                {
                    await Navigation.PushAsync(new EmailVerification());
                }
                else
                {
                    await DisplayAlert("Aviso", "Solo te quedan 2 intentos más.", "OK");
                }
            }
            else if (failedAttempts >= 5)
            {
                await DisplayAlert("Cuenta bloqueada", "Has superado el número máximo de intentos. Tu cuenta está bloqueada temporalmente.", "OK");
                await Navigation.PopToRootAsync(); // Regresa al login
            }
            else
            {
                int remaining = 5 - failedAttempts;
                await DisplayAlert("Código incorrecto", $"Código incorrecto. Te quedan {remaining} intentos.", "OK");
            }
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
            return email;

        string firstTwo = namePart.Substring(0, 2);
        string lastTwo = namePart.Substring(namePart.Length - 2);

        string masked = firstTwo + new string('*', namePart.Length - 4) + lastTwo;

        return $"{masked}@{domainPart}";
    }

    private async void OnNotYourAccountClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); // Regresa a EmailVerification
    }
}
