using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Data;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;
using LocalLink.Services.Validation;

namespace LocalLink.ViewModel.HomeViewModel
{
    public partial class AddProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly IUserFactory _factory;
        private readonly IValidationService _validationService;

        // --- Propiedades de Datos ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProductCommand))] private string _name;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProductCommand))] private string _description;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProductCommand))] private string _price;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProductCommand))] private string _stock = "1";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProductCommand))] private string _discountPercentage = "0";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveProductCommand))] private string _selectedCategory;

        // --- Propiedades de Error para la UI (Feedback en tiempo real) ---
        [ObservableProperty] private string _nameError;
        [ObservableProperty] private string _descriptionError;
        [ObservableProperty] private string _priceError;
        [ObservableProperty] private string _stockError;
        [ObservableProperty] private string _categoryError;

        // --- Propiedades de Imágenes ---
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectImageCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteImageCommand))]
        [NotifyPropertyChangedFor(nameof(HasImage1))]
        private ImageSource _image1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectImageCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteImageCommand))]
        [NotifyPropertyChangedFor(nameof(HasImage2))]
        private ImageSource _image2;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectImageCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteImageCommand))]
        [NotifyPropertyChangedFor(nameof(HasImage3))]
        private ImageSource _image3;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectImageCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteImageCommand))]
        [NotifyPropertyChangedFor(nameof(HasImage4))]
        private ImageSource _image4;

        // Helpers para visibilidad en XAML
        public bool HasImage1 => Image1 != null;
        public bool HasImage2 => Image2 != null;
        public bool HasImage3 => Image3 != null;
        public bool HasImage4 => Image4 != null;

        private List<string> _imagePaths = new List<string> { null, null, null, null };

        public AddProductViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService,
            IProductService productService,
            IUserFactory factory,
            IValidationService validationService)
            : base(connectivityService, navigationService, dialogService, userService)
        {
            _productService = productService;
            _factory = factory;
            _validationService = validationService;
        }

        [RelayCommand(CanExecute = nameof(CanSelectImage))]
        private async Task SelectImage(string index)
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo == null) return;

                // --- EL CAMBIO IMPORTANTE AQUÍ ---
                // Guardamos la ruta física en nuestra lista de strings
                int i = int.Parse(index) - 1;
                _imagePaths[i] = photo.FullPath;

                var stream = await photo.OpenReadAsync();
                var imgSource = ImageSource.FromStream(() => stream);

                switch (index)
                {
                    case "1": Image1 = imgSource; break;
                    case "2": Image2 = imgSource; break;
                    case "3": Image3 = imgSource; break;
                    case "4": Image4 = imgSource; break;
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error", "No se pudo cargar la imagen.", "OK");
            }
        }

        // Solo permite seleccionar si el espacio está vacío
        private bool CanSelectImage(string index) => GetImageByIndex(index) == null;

        [RelayCommand(CanExecute = nameof(CanDeleteImage))]
        private void DeleteImage(string index)
        {
            int i = int.Parse(index) - 1;
            _imagePaths[i] = null; // Limpiamos la ruta

            switch (index)
            {
                case "1": Image1 = null; break;
                case "2": Image2 = null; break;
                case "3": Image3 = null; break;
                case "4": Image4 = null; break;
            }
        }

        // Solo permite borrar si hay una imagen
        private bool CanDeleteImage(string index) => GetImageByIndex(index) != null;

        private ImageSource GetImageByIndex(string index)
        {
            return index switch
            {
                "1" => Image1,
                "2" => Image2,
                "3" => Image3,
                "4" => Image4,
                _ => null
            };
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveProduct()
        {
            if (!ConnectivityService.IsConnected)
            {
                await DialogService.ShowAlertAsync("Sin Conexión", "Revisa tu internet para publicar.", "Aceptar");
                return;
            }

            try
            {
                IsBusy = true;

                // 1. Obtener valores validados
                decimal.TryParse(Price, out decimal validatedPrice);
                int.TryParse(Stock, out int validatedStock);
                decimal.TryParse(DiscountPercentage, out decimal d);

                // 2. Crear objeto mediante Factory
                var newProduct = _factory.CreateProduct(
                    Name,
                    Description,
                    validatedPrice,
                    validatedStock,
                    Guid.NewGuid(),
                    SelectedCategory,
                    UserService.CurrentUser.Id,
                    d
                );

                // 3. Asignar rutas de imágenes (Esto ahora sí funcionará porque la lista tiene datos)
                foreach (var path in _imagePaths.Where(x => x != null))
                {
                    newProduct.Images.Add(new ProductImage
                    {
                        ImageUrl = path,
                        ProductId = newProduct.Id
                    });
                }

                // 4. Persistencia
                var success = await _productService.SaveProductAsync(newProduct);

                if (success)
                {
                    await DialogService.ShowAlertAsync("¡Éxito!", "Producto publicado correctamente.", "Aceptar");
                    await NavigationService.GoBackAsync();
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Error Crítico", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave()
        {
            if (IsBusy) return false;
            if (_validationService == null) return false;

            // 1. Obtener valores limpios
            string name = Name ?? string.Empty;
            string desc = Description ?? string.Empty;
            string price = Price ?? string.Empty;
            string stock = Stock ?? string.Empty;
            string cat = SelectedCategory ?? string.Empty;

            // 2. Ejecutar validaciones mediante servicio
            var vName = _validationService.ValidateProductName(name);
            var vDesc = _validationService.ValidateDescription(desc);
            var vPrice = _validationService.ValidatePrice(price, out _);
            var vStock = _validationService.ValidateStock(stock, out _);

            // 3. Gestión de Mensajes de Error (Feedback en tiempo real)
            NameError = !string.IsNullOrEmpty(name) && !vName.IsValid ? vName.Message : string.Empty;
            DescriptionError = !string.IsNullOrEmpty(desc) && !vDesc.IsValid ? vDesc.Message : string.Empty;
            PriceError = !string.IsNullOrEmpty(price) && !vPrice.IsValid ? vPrice.Message : string.Empty;
            StockError = !string.IsNullOrEmpty(stock) && !vStock.IsValid ? vStock.Message : string.Empty;
            CategoryError = string.IsNullOrEmpty(cat) ? "Selecciona una categoría." : string.Empty;

            // 4. Verificación de campos obligatorios llenos
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(name) &&
                                   !string.IsNullOrWhiteSpace(desc) &&
                                   !string.IsNullOrWhiteSpace(price) &&
                                   !string.IsNullOrWhiteSpace(stock) &&
                                   !string.IsNullOrWhiteSpace(cat);

            // 5. Activación del botón
            return allFieldsFilled && vName.IsValid && vDesc.IsValid && vPrice.IsValid && vStock.IsValid;
        }
    }
}