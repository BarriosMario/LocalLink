using LocalLink.ViewModel.LoginViewModel;

namespace LocalLink.View.Login;

public partial class GetCode : ContentPage
{
    public GetCode(GetCodeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnDigitTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        string val = e.NewTextValue;

        // --- LÓGICA DE BORRADO (RETROCESO) ---
        if (string.IsNullOrEmpty(val))
        {
            // Si el usuario borró el contenido, mandamos el foco al anterior
            if (entry == Digit6) Digit5.Focus();
            else if (entry == Digit5) Digit4.Focus();
            else if (entry == Digit4) Digit3.Focus();
            else if (entry == Digit3) Digit2.Focus();
            else if (entry == Digit2) Digit1.Focus();
            return;
        }

        // --- LÓGICA DE PEGADO (Varios dígitos) ---
        if (val.Length > 1)
        {
            string cleanCode = new string(val.Where(char.IsDigit).ToArray());

            if (cleanCode.Length >= 6)
            {
                Digit1.Text = cleanCode[0].ToString();
                Digit2.Text = cleanCode[1].ToString();
                Digit3.Text = cleanCode[2].ToString();
                Digit4.Text = cleanCode[3].ToString();
                Digit5.Text = cleanCode[4].ToString();
                Digit6.Text = cleanCode[5].ToString();

                Digit6.Focus();
                return;
            }
        }

        // --- LÓGICA DE AVANCE (Un solo dígito) ---
        if (val.Length == 1)
        {
            if (entry == Digit1) Digit2.Focus();
            else if (entry == Digit2) Digit3.Focus();
            else if (entry == Digit3) Digit4.Focus();
            else if (entry == Digit4) Digit5.Focus();
            else if (entry == Digit5) Digit6.Focus();
        }
    }
}