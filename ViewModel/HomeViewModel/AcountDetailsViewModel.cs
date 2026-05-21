using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;

namespace LocalLink.ViewModel.HomeViewModel
{
    public partial class AcountDetailsViewModel : BaseViewModel
    {
        public AcountDetailsViewModel(
            IConnectivityService connectivityService,
            INavigationService navigationService,
            IDialogService dialogService,
            IUserService userService)
            : base(connectivityService, navigationService, dialogService, userService) // Esto envía los servicios al BaseViewModel
        {
            // Aquí puedes inicializar cosas específicas
        }
    }
}
