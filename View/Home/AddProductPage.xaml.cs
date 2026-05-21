using LocalLink.ViewModel.HomeViewModel;

namespace LocalLink.View.Home;

public partial class AddProductPage : ContentPage
{
	public AddProductPage(AddProductViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;	
	}
}