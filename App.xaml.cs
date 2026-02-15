namespace LocalLink
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            bool isRegistered = Preferences.Default.Get("is_registered", false);

            if (isRegistered)
            {
                MainPage = new AppShell();
            }
            else
            {
                // Configuramos la NavigationPage con el color oscuro desde el inicio
                var loginPage = new LocalLink.Login.LoginPage();

                MainPage = new NavigationPage(loginPage)
                {
                    BarBackgroundColor = Color.FromArgb("#252525"),
                    BarTextColor = Colors.White
                };
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Retornamos la MainPage configurada (sea AppShell o NavigationPage)
            return new Window(MainPage);
        }
    }
}