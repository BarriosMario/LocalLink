using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLink.Services.Connectivity;
using LocalLink.Services.Dialogs;
using LocalLink.Services.Navigation;
using LocalLink.Services.Users;

public partial class BaseViewModel : ObservableObject
{
    protected readonly IConnectivityService ConnectivityService;
    protected readonly INavigationService NavigationService;
    protected readonly IDialogService DialogService;
    protected readonly IUserService UserService;

    [ObservableProperty]
    private bool isBusy;

    public BaseViewModel(
        IConnectivityService connectivityService,
        INavigationService navigationService,
        IDialogService dialogServic,
        IUserService userService)
    {
        ConnectivityService = connectivityService;
        NavigationService = navigationService;
        DialogService = dialogServic;
        UserService = userService;
    }
    public bool IsConnected => ConnectivityService.IsConnected;

    [RelayCommand]
    public async Task GoBack() => await NavigationService.GoBackAsync();

    [RelayCommand]
    public async Task GoHome() => await NavigationService.GoToAsync("//HomePage");

    [RelayCommand]
    public async Task GoLogin() => await NavigationService.GoToAsync("//LoginPage");
}
