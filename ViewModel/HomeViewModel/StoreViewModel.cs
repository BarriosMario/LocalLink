using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;
using LocalLink.Model; // Asegúrate de tener tu modelo
using System.Collections.ObjectModel;

namespace LocalLink.ViewModel.HomeViewModel
{
    public partial class StoreViewModel : BaseViewModel
    {
        private readonly IProductService _productService;

        [ObservableProperty]
        private ObservableCollection<Product> products;

        public StoreViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IProductService productService)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _productService = productService;
            Products = new ObservableCollection<Product>();

            // Eliminamos la llamada directa aquí para evitar doble carga
            // ya que OnAppearing se encargará de la primera carga.
        }

        // Nuevo comando para vincular al OnAppearing de la página
        [RelayCommand]
        private async Task AppearingAsync()
        {
            await LoadSellerProductsAsync();
        }

        [RelayCommand]
        public async Task LoadSellerProductsAsync() // Cambiado a public para acceso externo si es necesario
        {
            if (IsBusy) return;

            if (!ConnectivityService.IsConnected)
            {
                // Solo alertar si realmente no hay datos locales o es la primera vez
                // para no ser intrusivos cada vez que el usuario entra a la pestaña.
            }

            try
            {
                IsBusy = true;

                var result = await _productService.GetMyProductsAsync();

                // Ordenamos por fecha
                var sortedList = result.OrderByDescending(p => p.CreatedAt).ToList();

                // Optimizamos la actualización de la colección para evitar parpadeos
                Products.Clear();
                foreach (var product in sortedList)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error", "No se pudieron recuperar tus productos.", "OK");
                System.Diagnostics.Debug.WriteLine($"Error en StoreViewModel: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToAddProductAsync()
        {
            await NavigationService.GoToAsync("AddProductPage");
        }
    }
}