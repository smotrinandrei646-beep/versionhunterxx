namespace VersionHunterX;
public partial class HistoryPage : ContentPage
{
    public HistoryPage() { InitializeComponent(); List.ItemsSource = Store.History; }
    void OnClear(object? s, EventArgs e) { Store.History.Clear(); Store.Save(); }
    async void OnSelected(object? s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string q)
        {
            ((CollectionView)s!).SelectedItem = null;
            await Shell.Current.GoToAsync($"//search?q={Uri.EscapeDataString(q)}");
        }
    }
}
