namespace LocalLink.Politicas;

public partial class TermsAndConditions : ContentPage
{
	public TermsAndConditions()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Maneja el evento de clic para regresar a la página anterior.
    /// </summary>
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Verifica si hay páginas en la pila de navegación para evitar errores
        if (Navigation.NavigationStack.Count > 1)
        {
            // PopAsync quita la página actual y muestra la anterior
            await Navigation.PopAsync();
        }
        else
        {
            // En caso de que se haya abierto de forma modal o sea la única página
            // se puede usar Shell para ir a una ruta específica si usas AppShell
            await Shell.Current.GoToAsync("..");
        }
    }
}