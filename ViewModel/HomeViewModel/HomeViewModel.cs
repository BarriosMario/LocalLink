using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Model;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Data;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;

namespace LocalLink.ViewModel.HomeViewModel
{
    // Clase de soporte para empaquetar los datos de cada sección
    public class HomeSection
    {
        public string Title { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ICommand SeeMoreCommand { get; set; }
    }

    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IProductService _productService;

        [ObservableProperty]
        private ObservableCollection<Product> _allProducts = new();

        // --- PROPIEDADES DE SECCIÓN (Para el BindingContext de la Vista) ---
        [ObservableProperty] private HomeSection _newProductsSection;
        [ObservableProperty] private HomeSection _saleProductsSection;
        [ObservableProperty] private HomeSection _electronicaSection;
        [ObservableProperty] private HomeSection _ropaSection;
        [ObservableProperty] private HomeSection _hogarSection;
        [ObservableProperty] private HomeSection _comidaSection;
        [ObservableProperty] private HomeSection _mascotasSection;
        [ObservableProperty] private HomeSection _serviciosSection;
        [ObservableProperty] private HomeSection _otrosSection;

        public HomeViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IProductService productService)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _productService = productService;
            _ = LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsBusy = true;
                var products = await _productService.GetAllProductsAsync();

                // 1. Guardamos y ordenamos la fuente de datos principal
                AllProducts = new ObservableCollection<Product>(products.OrderByDescending(p => p.CreatedAt));

                // 2. Creamos las secciones usando el método auxiliar
                NewProductsSection = CreateSection("Recién Llegados", AllProducts.Where(p => p.IsNewProduct));
                SaleProductsSection = CreateSection("Ofertas Imperdibles", AllProducts.Where(p => p.IsOnSale));

                ElectronicaSection = CreateSection("Electrónica", AllProducts.Where(p => p.SelectedCategory == "Electrónica"));
                RopaSection = CreateSection("Ropa y Calzado", AllProducts.Where(p => p.SelectedCategory == "Ropa y Calzado"));
                HogarSection = CreateSection("Hogar", AllProducts.Where(p => p.SelectedCategory == "Hogar"));
                ComidaSection = CreateSection("Comida y Bebida", AllProducts.Where(p => p.SelectedCategory == "Comida y Bebida"));
                MascotasSection = CreateSection("Mascotas", AllProducts.Where(p => p.SelectedCategory == "Mascotas"));
                ServiciosSection = CreateSection("Servicios", AllProducts.Where(p => p.SelectedCategory == "Servicios"));
                OtrosSection = CreateSection("Otros", AllProducts.Where(p => p.SelectedCategory == "Otros"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LoadProducts: {ex.Message}");
                await DialogService.ShowAlertAsync("Error", "No se pudieron cargar los productos.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Método auxiliar para construir las secciones de forma estandarizada
        private HomeSection CreateSection(string title, IEnumerable<Product> query)
        {
            return new HomeSection
            {
                Title = title,
                Products = new ObservableCollection<Product>(query.Where(p => p.IsAvailable).Take(6)),
                //SeeMoreCommand = GoToCategoryPageCommand
            };
        }

        //[RelayCommand]
        //private async Task GoToCategoryPage(string categoryName)
        //{
        //    await NavigationService.GoToAsync("CategoryProductsPage", new Dictionary<string, object>
        //    {
        //        { "CategoryName", categoryName }
        //    });
        //}
    }
}