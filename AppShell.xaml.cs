namespace LocalLink
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registramos la ruta para poder navegar
            Routing.RegisterRoute("RegisterPage", typeof(LocalLink.Login.RegisterPage));
        }
    }
}
