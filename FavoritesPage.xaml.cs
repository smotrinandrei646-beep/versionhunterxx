namespace VersionHunterX;
public partial class FavoritesPage : ContentPage
{
    public FavoritesPage() { InitializeComponent(); List.ItemsSource = Store.Favorites; }
    async void OnSelected(object? s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Repo r) { ((CollectionView)s!).SelectedItem = null; await Navigation.PushAsync(new ReleasesPage(r)); }
    }
}
