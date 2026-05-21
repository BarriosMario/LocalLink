using LocalLink.ViewModel;

namespace LocalLink.View;

public partial class LoadingPage : ContentPage
{
	public LoadingPage(LoadingViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Al aparecer la página, ejecutamos el comando Init del ViewModel
        if (BindingContext is LoadingViewModel vm)
        {
            _ = vm.StartTipsRotation();
            await vm.InitCommand.ExecuteAsync(null);
        }
    }
}