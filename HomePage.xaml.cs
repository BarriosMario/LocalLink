<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vistas="clr-namespace:LocalLink.Vistas"
             x:Class="LocalLink.HomePage"
             Title="Marketplace">

    <ScrollView>
        <VerticalStackLayout Padding="15" Spacing="20">

            <SearchBar Placeholder="¿Buscas algo específico?" 
                       BackgroundColor="{AppThemeBinding Light=#F0F0F0, Dark=#202020}" />

            <Label Text="Novedades en LocalLink" FontSize="20" FontAttributes="Bold" />

            <CollectionView x:Name="ProductsCollection">
                <CollectionView.ItemsLayout>
                    <GridItemsLayout Orientation="Vertical" Span="2" 
                         HorizontalItemSpacing="10" VerticalItemSpacing="15" />
                </CollectionView.ItemsLayout>

                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <VerticalStackLayout Spacing="5">
                            <Border StrokeShape="RoundRectangle 15"
        StrokeThickness="0"
        HeightRequest="150"
        HorizontalOptions="Fill">
                                <Image Source="{Binding ImageUrl}" 
           Aspect="AspectFill" />
                            </Border>
                            <Label Text="{Binding Name}" FontAttributes="Bold" Margin="5,0" />
                            <Label Text="{Binding Price}" TextColor="#2196F3" Margin="5,0,5,10" />
                        </VerticalStackLayout>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
