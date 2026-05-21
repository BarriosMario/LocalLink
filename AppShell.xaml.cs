using LocalLink.Services.Navigation;

namespace LocalLink
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registramos las rutas de la aplicación
            AppRoutes.RegisterRoutes();

            // Lógica de navegación existente para Windows
            // Detecta cambios en el CurrentItem para resetear el Stack
            this.PropertyChanged += OnShellPropertyChanged;
        }

        private void OnShellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Cuando se interactúa con el TabBar, cambia el CurrentItem
            if (e.PropertyName == nameof(CurrentItem))
            {
                // Si ya estamos en el Tab pero tenemos páginas encima (Stack > 1)
                if (Navigation.NavigationStack.Count > 1)
                {
                    ResetToRoot();
                }
            }
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Esto funciona mayormente en Android/iOS para detectar navegación al mismo destino
            if (args.Current != null && args.Target != null &&
                args.Current.Location.OriginalString == args.Target.Location.OriginalString)
            {
                ResetToRoot();
            }
        }

        private void ResetToRoot()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Limpiamos el Stack de navegación para volver a la raíz de la pestaña
                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopToRootAsync(true);
                }
            });
        }
    }
}