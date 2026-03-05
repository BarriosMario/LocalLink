using LocalLink.Vistas.Profile;

namespace LocalLink
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registramos la ruta para poder navegar
            Routing.RegisterRoute(nameof(Login.LoginPage), typeof(Login.LoginPage));
            Routing.RegisterRoute(nameof(ProfileDetailsPage), typeof(ProfileDetailsPage));
        }
    }
}
