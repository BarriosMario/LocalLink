using LocalLink.Models;
namespace LocalLink;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();

        var productos = new List<Product>
        {
            new Product { Name = "Café", Price = "$102.00", ImageUrl = "dotnet_bot.png" },
            new Product { Name = "Laptop", Price = "$11000.00", ImageUrl = "dotnet_bot.png" },
            new Product { Name = "Bicicleta", Price = "$2000.00", ImageUrl = "dotnet_bot.png" },
            new Product { Name = "Pantalón", Price = "$200.00", ImageUrl = "dotnet_bot.png" }
        };
    }
}
